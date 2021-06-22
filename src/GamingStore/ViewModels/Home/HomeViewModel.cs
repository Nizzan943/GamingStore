using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        public List<Item> Items { get; set; }
    }
}
