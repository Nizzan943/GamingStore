using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GamingStore.Contracts
{
    public abstract class Address
    {
        public string FullName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string DeliveryNotes { get; set; }
        public string PhoneNumber { get; set; }
        
    }


    public class DeliveryAddress : Address
    { }

    public class BillingAddress : Address
    {
        public string CompanyId { get; set; }

    }
}
