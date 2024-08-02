using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader
{
    public class CargoException: Exception
    {
        public CargoException() { }

        public CargoException(string message) : base(message)
        {
        }
    }
}
