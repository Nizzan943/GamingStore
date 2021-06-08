using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Administration
{
    public class ListStoresViewModel
    {
        public IEnumerable<Store> Stores { get; set; }
    }
}
