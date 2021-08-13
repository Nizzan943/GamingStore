using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Administration
{
    public class ListOrdersViewModel : ViewModelBase
    {
        public IEnumerable<Order> Orders { get; set; }
    }
}
