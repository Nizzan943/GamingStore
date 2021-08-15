using GamingStore.Contracts;
using GamingStore.Models;
using Microsoft.AspNetCore.Http;

namespace GamingStore.ViewModels.Items
{
    public class CreateItemViewModel : ViewModelBase
    {
        public Item Item { get; set; }
        


        public IFormFile File1 { set; get; }
        
        public IFormFile File2 { set; get; }
        
        public IFormFile File3 { set; get; }

        
        public bool PublishItemFlag { get; set; }
    }
}
