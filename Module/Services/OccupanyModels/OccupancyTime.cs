using System;

namespace Module.Services.OccupanyModels
{
    public class OccupancyTime
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
