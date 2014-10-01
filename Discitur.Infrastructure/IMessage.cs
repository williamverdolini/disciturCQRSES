using System;

namespace Discitur.Infrastructure
{
    public interface IMessage
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }
}
