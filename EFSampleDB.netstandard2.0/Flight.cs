using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.netstandard2_0
{
    public class Flight //To demonstrate InverseProperty attribute
    {
        public int FlightID { get; set; }
        public string Name { get; set; }
        [InverseProperty("DepartingFlights")]
        public Airport DepartureAirport { get; set; }
        [InverseProperty("ArrivingFlights")]
        public Airport ArrivalAirport { get; set; }
    }
}
