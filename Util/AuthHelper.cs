using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ResourceHealthAlertPOC.Util
{
    class AuthHelper
    {
        public static async Task<string> GetTokenAsync()
        {
            
            string clientId =  Environment.GetEnvironmentVariable("ClientId", EnvironmentVariableTarget.Process);
            string clientSecret =  Environment.GetEnvironmentVariable("ClientSecret", EnvironmentVariableTarget.Process);
            string tenantId =  Environment.GetEnvironmentVariable("TenantId", EnvironmentVariableTarget.Process);
            string adUrl =  Environment.GetEnvironmentVariable("AuthenticationUrl", EnvironmentVariableTarget.Process);
            string managementUrl = Environment.GetEnvironmentVariable("ManagementUrl", EnvironmentVariableTarget.Process);

            ClientCredential cc = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(adUrl + tenantId);
            var result = await context.AcquireTokenAsync(managementUrl, cc);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            return result.AccessToken;
        }

    }
}