using System.Collections.Generic;
using GamingStore.Contracts;
using GamingStore.Models;

namespace GamingStore.ViewModels.Items
{
    public class GetChosenItemsViewModel : ViewModelBase
    {
        public Item[] Items { get; set; }
        public PaginatedList<Item> PaginatedItems { get; set; }
        public IEnumerable<string>  Categories { get; set; }
        public IEnumerable<string> Brands { get; set; }
        public string QueryTitle { get; set; }
        public double Price { get; set; }

        public Item[] AllItems { get; set; }
        public IEnumerable<string> AllCategories { get; set; }
        public IEnumerable<string> AllBrands { get; set; }

    }
}
