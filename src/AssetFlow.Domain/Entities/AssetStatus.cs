using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFlow.Domain.Entities
{
    public enum AssetStatus
    {
        Pending = 1,
        Uploaded = 2,
        Processing = 3,
        Processed = 4,
        Failed = 5
    }
}
