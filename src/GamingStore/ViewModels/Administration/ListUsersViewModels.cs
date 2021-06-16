using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Administration
{
    public class ListUsersViewModels
    {
        public List<User> Users { get; set; }
        public User CurrentUser { get; set; }
        public IList<string> CurrentUserRoles { get; set; }
    }
}