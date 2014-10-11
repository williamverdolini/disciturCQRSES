using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Legacy.Migration.Infrastructure.Exceptions
{
    public class RecoverableException : Exception
    {
        public RecoverableException() : base() { }
        public RecoverableException(string message) : base(message) { }
    }
}
