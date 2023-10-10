﻿using System;
using Azure;
using Azure.Data.Tables;

namespace PoC.BlobTriggerDurable.Failover
{
    public class TableLogEntry : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}