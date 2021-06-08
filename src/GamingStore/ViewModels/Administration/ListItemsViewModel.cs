using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Administration
{
    public class ListItemsViewModel
    {
        public IEnumerable<Item> Items { get; set; }
    }
}
