namespace SensorX.Master.Domain.ValueObjects
{
    public record Geolocation
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public Geolocation(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude),"Vĩ độ phải nằm trong khoảng -90 đến 90 độ.");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Kinh độ phải nằm trong khoảng -180 đến 180 độ.");
            }
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}