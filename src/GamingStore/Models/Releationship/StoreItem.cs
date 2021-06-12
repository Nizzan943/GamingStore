namespace GamingStore.Models.Relationships
{
    public class StoreItem
    {
        public int StoreId { get; set; }
        public Store Store { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public uint ItemsCount { get; set; }
    }
}