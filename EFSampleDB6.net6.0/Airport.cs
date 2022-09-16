using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFSampleDB6.net6_0
{
    public class Airport //To demonstrate InverseProperty attribute
    {
        public int AirportID { get; set; }
        public string Name { get; set; }

        [InverseProperty("DepartureAirport")]
        public virtual ICollection<Flight> DepartingFlights { get; set; }

        [InverseProperty("ArrivalAirport")]
        public virtual ICollection<Flight> ArrivingFlights { get; set; }
    }

}
