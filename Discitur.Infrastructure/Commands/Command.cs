using System;

namespace Discitur.Infrastructure.Commands
{
    [Serializable]
    public abstract class Command : IMessage
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
