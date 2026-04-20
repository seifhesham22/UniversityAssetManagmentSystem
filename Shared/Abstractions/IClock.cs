using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}