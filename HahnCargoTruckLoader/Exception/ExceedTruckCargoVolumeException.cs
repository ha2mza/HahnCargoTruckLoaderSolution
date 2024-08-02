using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader
{
    public class ExceedTruckCargoVolumeException : Exception
    {
        public ExceedTruckCargoVolumeException() { }
        public ExceedTruckCargoVolumeException(string message) : base(message) { }
    }
}
