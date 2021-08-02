using System.Collections.Generic;
using GamingStore.Contracts;
using GamingStore.Models;

namespace GamingStore.ViewModels.Items
{
    public class GetItemsViewModel : ViewModelBase
    {
        public Item[] Items { get; set; }
        public PaginatedList<Item> PaginatedItems { get; set; }
        public IEnumerable<string>  Categories { get; set; }
        public IEnumerable<string> Brands { get; set; }
    }
}
