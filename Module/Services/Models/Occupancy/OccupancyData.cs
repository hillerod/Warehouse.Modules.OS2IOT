using System;

namespace Module.Services.Models.Occupancy
{
    public class OccupancyData
    {
        private int _occupancy;

        public DateTime Time { get; set; }

        public int Occupancy
        {
            get { return _occupancy; }
            set { _occupancy = value >= 1 ? 1 : 0; }
        }
    }
}
