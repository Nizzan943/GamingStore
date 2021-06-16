using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Stores
{
    public class StoresCitiesViewModel : ViewModelBase
    {
        public IEnumerable<Store> Stores { get; set; }

        public string[] CitiesWithStores { get; set; }
        public IEnumerable<Store> OpenStores{ get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public bool IsOpen { get; set; } 
    }
}
