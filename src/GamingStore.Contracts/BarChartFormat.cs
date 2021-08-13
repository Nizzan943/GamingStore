using System.ComponentModel.DataAnnotations;


namespace GamingStore.Contracts
{
    class BarChartFormat
    {

        [DataType(DataType.Date)]
        public string Date { get; set; }

        [DataType(DataType.Currency)]
        public double Value { get; set; }
    }
}
