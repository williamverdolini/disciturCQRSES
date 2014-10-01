using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Infrastructure
{
    public class Contract
    {
        public static void Requires<TException>(bool Predicate, string Message)
            where TException : Exception, new()
        {
            if (!Predicate)
            {
                Debug.WriteLine(Message);
                throw new TException();
            }
        }
    } 
}
