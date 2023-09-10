using System;

namespace Module.Services.OccupanyModels
{
    public class OccupancyTime
    {
        public DateTime Time { get; set; }

        //public int Occupancy { get; set; }

        private int _occupancy;

        public int Occupancy
        {
            get { return _occupancy; }
            set { _occupancy = value > 1 ? 1 : 0; }  //DEtte er helt forkert!!!!!!!!!
        }

    }
}
