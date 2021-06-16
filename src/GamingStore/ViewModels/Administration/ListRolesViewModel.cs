using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace GamingStore.ViewModels.Administration
{
    public class ListRolesViewModel : ViewModelBase
    {
        public IQueryable<IdentityRole> Roles { get; set; }
    }
}