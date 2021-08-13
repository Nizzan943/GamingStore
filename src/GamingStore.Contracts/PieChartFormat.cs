using System.ComponentModel.DataAnnotations;


namespace GamingStore.Contracts
{
    public class PieChartFormat
    {
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public double Value { get; set; }
    }
}
