using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Administration
{
    public class IndexViewModel :ViewModelBase
    {
        public IEnumerable<Store> Stores { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public IEnumerable<User> Users{ get; set; }
        public IEnumerable<Order> Orders{ get; set; }

        public Dictionary<string,double> WidgetsValues{ get; set; }
    }
}