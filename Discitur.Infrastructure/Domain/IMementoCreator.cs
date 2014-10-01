using CommonDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Infrastructure.Domain
{
    public interface IMementoCreator
    {
        IMemento CreateMemento();
    }
}
