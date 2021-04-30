using System.ComponentModel.DataAnnotations;

namespace GamingStore.Contracts
{
    public enum OrderState
    {
        New,
        [Display(Name = "In Progress")]
        InProgress,
        Shipped,
        Fulfilled,
        Cancelled
    }
}