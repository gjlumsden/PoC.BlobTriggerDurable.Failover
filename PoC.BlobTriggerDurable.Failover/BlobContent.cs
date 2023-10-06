using System.Collections.Generic;

namespace PoC.BlobTriggerDurable.Failover
{
    public class BlobContent
    {
        public string Filename { get; set; }
        public List<string> Lines { get; set; }
    }
}