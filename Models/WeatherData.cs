namespace WeatherApp.Models
{
    public class WeatherData
    {
        public string City { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public string WindDescription { get; set; }
        public double Pressure { get; set; }
        public double UVIndex { get; set; }
        public double DewPoint { get; set; }
        public double Visibility { get; set; }

        // Optional for display formatting
        public string WindDirectionText => WindDirection;
    }
}
