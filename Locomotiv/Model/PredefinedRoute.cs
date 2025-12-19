using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model
{
    public class PredefinedRoute
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Station StartStation { get; set; }

        public Station EndStation { get; set; }

        public List<int> BlockIds { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public TimeSpan Duration => ArrivalTime - DepartureTime;

        public decimal Price { get; set; }

        public string IntermediateStations { get; set; } = string.Empty;
    }
}
