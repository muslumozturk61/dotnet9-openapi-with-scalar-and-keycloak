using System.ComponentModel;

namespace ScalarSample.Api
{
    public class WeatherForecast
    {
        [Description("This is a Date.")]
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [Description("This is a Summary.")]
        public string? Summary { get; set; }
    }
}
