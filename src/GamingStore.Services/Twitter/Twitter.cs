using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GamingStore.Services.Twitter
{
    public class Twitter
    {
        readonly string _consumerKey;
        readonly string _consumerKeySecret;
        readonly string _accessToken;
        readonly string _accessTokenSecret;
        readonly HMACSHA1 _sigHasher;
        readonly DateTime _epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Twitter endpoint for sending tweets
        /// </summary>
        readonly string _TwitterTextAPI;

        /// <summary>
        /// Twitter endpoint for uploading images
        /// </summary>
        readonly string _TwitterImageAPI;

        /// <summary>
        /// Current tweet limit
        /// </summary>
        readonly int _limit;

        public Twitter(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret,
            int limit = 280)
        {
            _TwitterTextAPI = "https://api.twitter.com/1.1/statuses/update.json";
            _TwitterImageAPI = "https://upload.twitter.com/1.1/media/upload.json";

            _consumerKey = consumerKey;
            _consumerKeySecret = consumerKeySecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _limit = limit;

            _sigHasher = new HMACSHA1(new ASCIIEncoding().GetBytes($"{_consumerKeySecret}&{_accessTokenSecret}"));
        }

        /// <summary>
        /// Publish a post with image
        /// </summary>
        /// <returns>result</returns>
        /// <param name="post">post to publish</param>
        /// <param name="pathToImage">image to attach</param>
        public string PublishToTwitter(string post, string pathToImage)
        {
            try
            {
                // first, upload the image
                string mediaID = string.Empty;
                var rezImage = Task.Run(async () =>
                {
                    var response = await TweetImage(pathToImage);
                    return response;
                });
                var rezImageJson = JObject.Parse(rezImage.Result.Item2);

                if (rezImage.Result.Item1 != 200)
                {
                    try // return error from JSON
                    {
                        return
                            $"Error uploading image to Twitter. {rezImageJson["errors"][0]["message"].Value<string>()}";
                    }
                    catch (Exception ex) // return unknown error
                    {
                        // log exception somewhere
                        return "Unknown error uploading image to Twitter";
                    }
                }

                mediaID = rezImageJson["media_id_string"].Value<string>();

                // second, send the text with the uploaded image
                var rezText = Task.Run(async () =>
                {
                    var response = await TweetText(CutTweetToLimit(post), mediaID);
                    return response;
                });
                var rezTextJson = JObject.Parse(rezText.Result.Item2);

                if (rezText.Result.Item1 != 200)
                {
                    try // return error from JSON
                    {
                        return $"Error sending post to Twitter. {rezTextJson["errors"][0]["message"].Value<string>()}";
                    }
                    catch (Exception) // return unknown error
                    {
                        // log exception somewhere
                        return "Unknown error sending post to Twitter";
                    }
                }

                return "OK";
            }
            catch (Exception)
            {
                // log exception somewhere
                return "Unknown error publishing to Twitter";
            }
        }

        /// <summary>
        /// Send a tweet with some image attached
        /// </summary>
        /// <returns>HTTP StatusCode and response</returns>
        /// <param name="text">Text</param>
        /// <param name="mediaID">Media ID for the uploaded image. Pass empty string, if you want to send just text</param>
        public Task<Tuple<int, string>> TweetText(string text, string mediaID)
        {
            var textData = new Dictionary<string, string>
            {
                {"status", text},
                {"trim_user", "1"},
                {"media_ids", mediaID}
            };

            return SendText(_TwitterTextAPI, textData);
        }

        /// <summary>
        /// Upload some image to Twitter
        /// </summary>
        /// <returns>HTTP StatusCode and response</returns>
        /// <param name="pathToImage">Path to the image to send</param>
        public Task<Tuple<int, string>> TweetImage(string pathToImage)
        {
            byte[] imgdata = System.IO.File.ReadAllBytes(pathToImage);
            var imageContent = new ByteArrayContent(imgdata);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(imageContent, "media");

            return SendImage(_TwitterImageAPI, multipartContent);
        }

        async Task<Tuple<int, string>> SendText(string URL, Dictionary<string, string> textData)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", PrepareOAuth(URL, textData, "POST"));

                var httpResponse = await httpClient.PostAsync(URL, new FormUrlEncodedContent(textData));
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>((int) httpResponse.StatusCode, httpContent);
            }
        }

        async Task<Tuple<int, string>> SendImage(string URL, MultipartFormDataContent multipartContent)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", PrepareOAuth(URL, null, "POST"));

                var httpResponse = await httpClient.PostAsync(URL, multipartContent);
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>((int) httpResponse.StatusCode, httpContent);
            }
        }

        #region Some OAuth magic

        string PrepareOAuth(string URL, Dictionary<string, string> data, string httpMethod)
        {
            // seconds passed since 1/1/1970
            var timestamp = (int) ((DateTime.UtcNow - _epochUtc).TotalSeconds);

            // Add all the OAuth headers we'll need to use when constructing the hash
            Dictionary<string, string> oAuthData = new Dictionary<string, string>();
            oAuthData.Add("oauth_consumer_key", _consumerKey);
            oAuthData.Add("oauth_signature_method", "HMAC-SHA1");
            oAuthData.Add("oauth_timestamp", timestamp.ToString());
            oAuthData.Add("oauth_nonce", Guid.NewGuid().ToString());
            oAuthData.Add("oauth_token", _accessToken);
            oAuthData.Add("oauth_version", "1.0");

            if (data != null) // add text data too, because it is a part of the signature
            {
                foreach (var item in data)
                {
                    oAuthData.Add(item.Key, item.Value);
                }
            }

            // Generate the OAuth signature and add it to our payload
            oAuthData.Add("oauth_signature", GenerateSignature(URL, oAuthData, httpMethod));

            // Build the OAuth HTTP Header from the data
            return GenerateOAuthHeader(oAuthData);
        }

        /// <summary>
        /// Generate an OAuth signature from OAuth header values
        /// </summary>
        string GenerateSignature(string url, Dictionary<string, string> data, string httpMethod)
        {
            var sigString = string.Join("&",
                data.Union(data).Select(kvp =>
                        string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s));

            var fullSigData = string.Format("{0}&{1}&{2}", httpMethod, Uri.EscapeDataString(url),
                Uri.EscapeDataString(sigString.ToString()));

            return Convert.ToBase64String(_sigHasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString())));
        }

        /// <summary>
        /// Generate the raw OAuth HTML header from the values (including signature)
        /// </summary>
        string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            return string.Format("OAuth {0}",
                string.Join(", ",
                    data.Where(kvp => kvp.Key.StartsWith("oauth_")).Select(kvp =>
                            string.Format("{0}=\"{1}\"", Uri.EscapeDataString(kvp.Key),
                                Uri.EscapeDataString(kvp.Value)))
                        .OrderBy(s => s)));
        }

        #endregion

        /// <summary>
        /// Cuts the tweet text to fit the limit
        /// </summary>
        /// <returns>Cutted tweet text</returns>
        /// <param name="tweet">Uncutted tweet text</param>
        string CutTweetToLimit(string tweet)
        {
            while (tweet.Length >= _limit)
            {
                tweet = tweet.Substring(0, tweet.LastIndexOf(" ", StringComparison.Ordinal));
            }

            return tweet;
        }
    }
}