using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GamingStore.Contracts
{
    public enum PaymentMethod
    {
        Cash,
        [Display(Name = "Credit Card")]
        CreditCard,
        Paypal
    }
}

