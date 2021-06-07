using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GamingStore.ViewModels
{
    public  class ViewModelBase 
    {
        public int? ItemsInCart { get; set; } = null;
    }
}
