using System.Collections.Generic;

namespace GamingStore.ViewModels.Administration
{
    public class ListUserRoleViewModel
    {
        public List<UserRoleViewModel> List { get; set; }
        
        public string RoleName { get; set; }
    }
}