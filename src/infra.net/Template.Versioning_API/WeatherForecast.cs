namespace Modelo.Versionamento_API.v1_0
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    }
}
namespace Modelo.Versionamento_API.v2_0
{
    public class WeatherForecast : v1_0.WeatherForecast
    {
        public string Summary { get; set; }
    }
}
