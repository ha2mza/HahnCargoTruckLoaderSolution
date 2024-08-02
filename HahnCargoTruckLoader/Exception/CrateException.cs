using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader
{
    public class CrateException: Exception
    {
        public CrateException() { }
        public CrateException(string message) : base(message) { }
    }
}
