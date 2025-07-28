namespace Highever.SocialMedia.API
{
    /// <summary>
    /// ע��1
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// ע��2
        /// </summary>
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
