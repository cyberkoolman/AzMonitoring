using System;
using System.Collections.Generic;
using System.Text;
using ResourceHealthAlertPOC.Models;
using ActivityAlertPOC.Models;
using Microsoft.Extensions.Logging;

namespace ResourceHealthAlertPOC.Util
{
    public static class CosmosHelper
    {
        public static ResourceHealthDto GetResourceHealthDtoMapping(ResourceHealthAlert resource)
        {
             ResourceIDs resource_IDs =GetResourceIds(resource.data.essentials.alertTargetIDs[0]) ;

            var resourceDto = new ResourceHealthDto
            {
                id = resource_IDs.id,
                alertId = resource.data.essentials.alertId,
                alertStatus = resource.data.alertContext.status,
                resourceId = resource_IDs.resourceId,
                currentHealthStatus = resource.data.alertContext.properties.currentHealthStatus,
                previousHealthStatus = resource.data.alertContext.properties.previousHealthStatus,
                eventTimestamp = resource.data.alertContext.eventTimestamp,
                subscriptionId = resource.data.essentials.alertTargetIDs[0].Split("/")[2],
                
                resourceName = resource_IDs.resourceName,
                resourceType = resource_IDs.resourceType,
                
                resourceGroupName = (resource.data.essentials.alertTargetIDs[0].Split("/")[4]).ToLower(),
                summary = resource.data.alertContext.properties.title,
                cause = String.IsNullOrEmpty(resource.data.alertContext.properties.cause) ? "N/A" : resource.data.alertContext.properties.cause.ToString(),
                details = String.IsNullOrEmpty(resource.data.alertContext.properties.details) ? "N/A" : resource.data.alertContext.properties.details.ToString(),
                correlationId = resource.data.alertContext.correlationId,
                operationName = resource.data.alertContext.operationName.ToLower(),
                operationId = resource.data.alertContext.operationId


            };

            return resourceDto;
        }

        public static ActivityLogDto GetActivityLogAlertDtoMapping(ActivityLogAlert resource, ILogger log)
        {
            log.LogInformation(resource.ToString());

            log.LogInformation("GetActivityLogAlertDtoMapping");
              ResourceIDs resource_IDs =GetResourceIds(resource.data.essentials.alertTargetIDs[0]) ;
            var resourceDto = new ActivityLogDto();
            try{
                resourceDto.id = resource_IDs.id;
                resourceDto.alertId = resource.data.essentials.alertId;
                resourceDto.alertStatus = resource.data.alertContext.status;
                resourceDto.resourceId = resource_IDs.resourceId;
                resourceDto.alertStatus = resource.data.alertContext.status;
                resourceDto.eventTimestamp = resource.data.alertContext.eventTimestamp;
                resourceDto.correlationId = resource.data.alertContext.correlationId;
                resourceDto.operationName = resource.data.alertContext.operationName;
                resourceDto.operationId = resource.data.alertContext.operationId;
                resourceDto.subscriptionId = resource.data.essentials.alertTargetIDs[0].Split("/")[2];
                resourceDto.resourceName = resource_IDs.resourceName;
                resourceDto.resourceType = resource_IDs.resourceType;
                resourceDto.resourceGroupName = (resource.data.essentials.alertTargetIDs[0].Split("/")[4]).ToLower();

            }
            catch(Exception exe)
            {
                log.LogInformation(exe.Message);
            }
            return resourceDto;
       }

        public static ResourceHealthDto GetDtoHistoryMapping(ResourceHealthAlert resource)
        {
            ResourceIDs resource_IDs =GetResourceIds(resource.data.essentials.alertTargetIDs[0]) ;
            var resourceDto = new ResourceHealthDto
            {
                id = Guid.NewGuid().ToString(),
                alertId = resource.data.essentials.alertId,
                alertStatus = resource.data.alertContext.status,
                resourceId = resource_IDs.resourceId,
                currentHealthStatus = resource.data.alertContext.properties.currentHealthStatus,
                previousHealthStatus = resource.data.alertContext.properties.previousHealthStatus,
                eventTimestamp = resource.data.alertContext.eventTimestamp,
                subscriptionId = resource.data.essentials.alertTargetIDs[0].Split("/")[2],
                resourceName = resource_IDs.resourceName,
                resourceType = resource_IDs.resourceType,
                resourceGroupName = (resource.data.essentials.alertTargetIDs[0].Split("/")[4]).ToLower(),
                summary = resource.data.alertContext.properties.title,
                cause = String.IsNullOrEmpty(resource.data.alertContext.properties.cause) ? "N/A" : resource.data.alertContext.properties.cause.ToString(),
                details = String.IsNullOrEmpty(resource.data.alertContext.properties.details) ? "N/A" : resource.data.alertContext.properties.details.ToString(),
            
                correlationId = resource.data.alertContext.correlationId,
                operationName = resource.data.alertContext.operationName,
                operationId = resource.data.alertContext.operationId
            };

            return resourceDto;
        }
        public static ResourceHealthDto GetAvStatusHistoryDtoMapping(AvailabilityStatus availabilityStatus)
        {
             ResourceIDs resource_IDs =GetResourceIds(availabilityStatus.id) ;
             var resourceDto = new ResourceHealthDto
            {
                id = Guid.NewGuid().ToString(),
                alertId = "N/A",
                alertStatus = "N/A",
                resourceId = resource_IDs.resourceId,
                currentHealthStatus = availabilityStatus.properties.availabilityState,
                previousHealthStatus = "N/A",
                eventTimestamp = DateTime.UtcNow,
                subscriptionId = availabilityStatus.id.Split("/")[2],
                resourceGroupName = (availabilityStatus.id.Split("/")[4]).ToLower(),
                resourceType = resource_IDs.resourceType,
                resourceName = resource_IDs.resourceName,
                summary = availabilityStatus.properties.summary,
                details = availabilityStatus.properties.detailedStatus,
                location = availabilityStatus.location
            };
            return resourceDto;
        }

        public static ResourceHealthDto GetAvStatusDtoMapping(AvailabilityStatus availabilityStatus)
        {
             ResourceIDs resource_IDs =GetResourceIds(availabilityStatus.id) ;
            var resourceDto = new ResourceHealthDto
            {
                id = resource_IDs.id,
                alertId = "N/A",
                alertStatus = "N/A",
                resourceId = resource_IDs.resourceId,
                currentHealthStatus = availabilityStatus.properties.availabilityState,
                previousHealthStatus = "N/A",
                eventTimestamp = DateTime.UtcNow,
                subscriptionId = availabilityStatus.id.Split("/")[2],
                resourceGroupName = (availabilityStatus.id.Split("/")[4]).ToLower(),
                resourceType = resource_IDs.resourceType,
                resourceName = resource_IDs.resourceName,
                summary = availabilityStatus.properties.summary,
                details = availabilityStatus.properties.detailedStatus,
                location = availabilityStatus.location
            };

            return resourceDto;
        }


        public static ResourceIDs GetResourceIds(string uid)
        {
            ResourceIDs resource_ids= new ResourceIDs();
            //MSALEM Make Sure to remove unwanted character from ID sent by availabilty list
            uid = uid.ToLower().Replace("/providers/microsoft.resourcehealth","");
            string[] list = uid.Split("/");
            if (list.Length > 10)
            {
                resource_ids.resourceType= list[6] + "/" + list[7] +"/"+ list[9] ;  
                resource_ids.resourceName= list[10] ;  
                resource_ids.id= list[2] + "_" + list[4] +"_"+ list[8] +"_"+ list[10] ;  
                resource_ids.resourceId= "/" + list[1] + "/" + list[2] + "/" + list[3] + "/" + list[4] + "/" + list[5] + "/" + list[6] + "/" +list[7] + "/" + list[8] + "/" + list[9] + "/" + list[10];

            }
            else
            {
                resource_ids.resourceType= list[6] + "/" + list[7]  ;  
                resource_ids.resourceName= list[8] ;  
                resource_ids.id= list[2] + "_" + list[4] +"_"+ list[8] ;  
                resource_ids.resourceId="/" + list[1] + "/" + list[2] + "/" + list[3] + "/" + list[4] + "/" + list[5] + "/" + list[6] + "/" +list[7] + "/" + list[8];

            }


            return resource_ids;

            
        }


        
    }

    public class ResourceIDs 
        
        {
            public string id { get; set; }
            public string resourceName { get; set; }
            public string resourceType { get; set; }

            public string resourceId { get; set; }


        }
}
