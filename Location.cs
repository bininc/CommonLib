using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib
{
    public class Location : IFormattable
    {
        public const double MaxLatitude = 90.0;
        public const double MinLatitude = -90.0;
        public const double MaxLongitude = 180.0;
        public const double MinLongitude = -180.0;
        private double latitude;
        private double longitude;
        private double altitude;

        public double Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = value;
            }
        }


        public double Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = value;
            }
        }


        public double Altitude
        {
            get
            {
                return this.altitude;
            }
            set
            {
                this.altitude = value;
            }
        }



        public Location()
            : this(0.0, 0.0, 0.0)
        {
        }

        public Location(double latitude, double longitude)
            : this(latitude, longitude, 0.0)
        {
        }

        public Location(double latitude, double longitude, double altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
        }

        public Location(Location location)
        {
            this.Latitude = location.latitude;
            this.Longitude = location.Longitude;
            this.Altitude = location.Altitude;
        }

        public static bool operator ==(Location location1, Location location2)
        {
            if (object.ReferenceEquals((object)location1, (object)location2))
                return true;
            if (location1 == null || location2 == (Location)null || (!Location.IsEqual(location1.Latitude, location2.Latitude) || !Location.IsEqual(location1.Longitude, location2.Longitude)) || !Location.IsEqual(location1.Altitude, location2.Altitude))
                return false;
            return location1 == location2;
        }

        public static bool operator !=(Location location1, Location location2)
        {
            return !(location1 == location2);
        }

        public static double NormalizeLongitude(double longitude)
        {
            if (longitude < -180.0 || longitude > 180.0)
                return longitude - Math.Floor((longitude + 180.0) / 360.0) * 360.0;
            return longitude;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Location))
                return false;
            return this == (Location)obj;
        }

        public override int GetHashCode()
        {
            return this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode() ^ this.Altitude.GetHashCode();
        }

        public override string ToString()
        {
            return ((IFormattable)this).ToString((string)null, (IFormatProvider)null);
        }

        public string ToString(IFormatProvider provider)
        {
            return ((IFormattable)this).ToString((string)null, provider);
        }

        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return string.Format(provider, "{0:" + format + "},{1:" + format + "},{2:" + format + "}", new object[3]
      {
        (object) this.latitude,
        (object) this.longitude,
        (object) this.altitude
      });
        }

        private static bool IsEqual(double value1, double value2)
        {
            return Math.Abs(value1 - value2) <= double.Epsilon;
        }
    }
}
