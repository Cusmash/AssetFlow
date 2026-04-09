using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFlow.Infrastructure.Options
{
    public sealed class ServiceBusOptions
    {
        public const string SectionName = "ServiceBus";
        public string ConnectionString { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
    }
}
