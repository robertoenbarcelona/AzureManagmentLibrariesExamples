using Microsoft.Azure;
using Microsoft.Azure.Subscriptions.Models;
using Microsoft.WindowsAzure.Management.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureManageLib.ConsoleHost
{
    internal class VirtualNetworkBuilder
    {
        private readonly SubscriptionCloudCredentials credentials;

        public VirtualNetworkBuilder(SubscriptionCloudCredentials credentials)
        {
            this.credentials = credentials;
        }

        public async Task<StepResult> CreatePublicIPAddressAsync(string ipName, string serviceName, string deployName)
        {
            try
            {
                Console.WriteLine("Creando el ip publico ...");
                var networkManagementClient = new NetworkManagementClient(this.credentials);
                var res = await networkManagementClient.VirtualIPs.AddAsync(serviceName, deployName, ipName).ConfigureAwait(false);
                return new StepResult() { Succed = true, Message = "Storage creado" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"IP non creado: {ex.Message}" };
            }
        }
    }
}
