using GamingStore.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace GamingStore.ViewModels.Items
{
    public class CreateItemViewModel : ViewModelBase
    {
        public Item Item { get; set; }


        [Required(ErrorMessage = "the Primary Image field is required.")]
        public IFormFile File1 { set; get; }

        [Required(ErrorMessage = "the 2nd Image field is required.")]
        public IFormFile File2 { set; get; }

        [Required(ErrorMessage = "the 3nd Image field is required.")]
        public IFormFile File3 { set; get; }

        
        public bool PublishItemFlag { get; set; }
    }
}
