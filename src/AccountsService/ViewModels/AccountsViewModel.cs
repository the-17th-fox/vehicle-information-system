using AccountsService.Models;

namespace AccountsService.ViewModels
{
    public class AccountsViewModel
    {
        public List<User> Users { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}
