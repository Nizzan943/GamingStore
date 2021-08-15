using GamingStore.Contracts;
using GamingStore.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GamingStore.ViewModels.Items
{
    public class EditItemViewModel : ViewModelBase
    {
        public Item Item { get; set; }

        public string LastItemName { get; set; }

        public IFormFile File1 { set; get; }
        
        public IFormFile File2 { set; get; }
        
        public IFormFile File3 { set; get; }

        public IEnumerable<Category> categories { get; set; }
    }
}
