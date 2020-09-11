using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ResourceHealthAlertPOC.Models;


namespace ResourceHealthAlertPOC.Util
{
    public static class AlertHelper
    {
        public static ResourceAlert PopulateMetaData(ResourceAlert resource)
        {
            resource.data.resourceId = resource.data.resourceUri;
            resource.data.resourceName = resource.data.resourceUri.Split('/')[8];
            resource.data.resourceGroupName = resource.data.resourceUri.Split('/')[4];
            resource.id = resource.data.subscriptionId + "_" + resource.data.resourceGroupName + "_" + resource.data.resourceName;

            return resource;
        }

        public static ResourceAlertDto ConvertToDTO(ResourceAlert resource)
        {

            // override the resourcetype to make sure it is populated
            string m_resourceName = (resource.data.resourceUri.Split('/')[8]).ToLower();
            string m_resourceGroupName = (resource.data.resourceUri.Split('/')[4]).ToLower();
            string m_resourceType = resource.data.operationName.Replace("/write", "").ToLower();

            var resourceAlertDTO = new ResourceAlertDto()
            {
                alertId = Guid.NewGuid().ToString(),
                alertStatus = "Resolved",
                currentHealthStatus = "Available",
                previousHealthStatus = "null",
                eventTimestamp = resource.eventTime,
                resourceId = resource.data.resourceUri.ToLower(),
                id = (resource.data.resourceUri.Split("/")[2]+"_"+resource.data.resourceUri.Split("/")[4]+"_"+resource.data.resourceUri.Split("/")[8]).ToLower(),
                subscriptionId = resource.data.subscriptionId,
                operationId=resource.data.operationId,
                operationName=(resource.data.operationName).ToLower(),
                correlationId=resource.data.correlationId,
                resourceName=m_resourceName,
                resourceGroupName=m_resourceGroupName,
                resourceType=m_resourceType
            };

            return resourceAlertDTO;
        }
    
    }
}
