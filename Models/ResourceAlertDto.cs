using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceHealthAlertPOC.Models
{
    public class ResourceAlertDto
    {
            public string id { get; set; }
            public string alertId { get; set; }
            public string resourceId { get; set; }
            public string subscriptionId { get; set; }
            public string alertStatus { get; set; }
            public string resourceType { get; set; }
            public string resourceGroupName { get; set; }
            public string resourceName { get; set; }
            public string operationName { get; set; }
            public string operationId { get; set; }
            public string currentHealthStatus { get; set; }
            public string previousHealthStatus { get; set; }
            public DateTime eventTimestamp { get; set; }
            public string location {get; set;}
 
            public string correlationId {get; set;}
            
        }
}

