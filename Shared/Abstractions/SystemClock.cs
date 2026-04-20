using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Abstractions
{
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}