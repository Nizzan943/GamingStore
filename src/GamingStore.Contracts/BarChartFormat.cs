using System.ComponentModel.DataAnnotations;


namespace GamingStore.Contracts
{
    public class BarChartFormat
    {

        [DataType(DataType.Date)]
        public string Date { get; set; }

        [DataType(DataType.Currency)]
        public double Value { get; set; }
    }
}
