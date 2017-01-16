
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure.Management.Network;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Rest;
    using Microsoft.Azure.Management.Network.Models;
    using Microsoft.WindowsAzure.Management.Models;

    internal class VirtualNetworkBuilder
    {
        private readonly TokenCredentials credentials;
        private readonly string subscriptionId;

        public VirtualNetworkBuilder(TokenCredentials credentials, string subscriptionId)
        {
            this.credentials = credentials;
            this.subscriptionId = subscriptionId;
        }

        internal async Task<StepResult> CreatePublicIPAddressAsync(string ipName, string groupName)
        {
            try
            {
                Console.WriteLine("Creando el IP publico ...");
                using (var networkManagementClient = new NetworkManagementClient(this.credentials))
                {
                    var res = await networkManagementClient.PublicIPAddresses.CreateOrUpdateAsync(
                         groupName,
                         ipName,
                         new PublicIPAddress
                         {
                             Location = LocationNames.WestEurope,
                             PublicIPAllocationMethod = "Dynamic"
                         }
                       ).ConfigureAwait(false);
                }

                return new StepResult() { Succed = true, Message = "IP creado" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"IP non creado: {ex.Message}" };
            }
        }

        internal async Task<StepResult> CreateVirtualNetworkAsync(string vnetName, string subnetName, string groupName)
        {
            try
            {
                Console.WriteLine("Creando el virtual network...");
                using (var networkManagementClient = new NetworkManagementClient(this.credentials) { SubscriptionId = this.subscriptionId })
                {
                    var subnet = new Subnet
                    {
                        Name = subnetName,
                        AddressPrefix = "192.168.0.0/24"
                    };
                    var address = new AddressSpace
                    {
                        AddressPrefixes = new List<string> { "192.168.0.0/16" }
                    };
                    var res = await networkManagementClient.VirtualNetworks.CreateOrUpdateAsync(
                              groupName,
                              vnetName,
                              new VirtualNetwork
                              {
                                  Location = LocationNames.WestEurope,
                                  AddressSpace = address,
                                  Subnets = new List<Subnet> { subnet }
                              }
                    ).ConfigureAwait(false);
                }

                return new StepResult() { Succed = true, Message = "Network creado" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Netwwork non creado: {ex.Message}" };
            }

        }

        internal async Task<StepResult> CreateNetworkInterfaceAsync(string vnetName, string subnetName, string ipName, string nicName, string groupName)
        {
            Console.WriteLine("Creando la network interface...");
            try
            {
                using (var networkManagementClient = new NetworkManagementClient(this.credentials) { SubscriptionId = subscriptionId })
                {
                    var subnetResponse = await networkManagementClient.Subnets.GetAsync(groupName, vnetName, subnetName).ConfigureAwait(false);
                    var pubipResponse = await networkManagementClient.PublicIPAddresses.GetAsync(groupName, ipName).ConfigureAwait(false);
                    var res = await networkManagementClient.NetworkInterfaces
                              .CreateOrUpdateAsync(
                                  groupName,
                                  nicName,
                                  new NetworkInterface
                                  {
                                      Location = LocationNames.WestEurope,
                                      IpConfigurations = new List<NetworkInterfaceIPConfiguration>
                                        {
                                         new NetworkInterfaceIPConfiguration
                                         {
                                           Name = nicName,
                                           PublicIPAddress = pubipResponse,
                                           Subnet = subnetResponse
                                         }
                                        }
                                  })
                            .ConfigureAwait(false);
                }

                return new StepResult() { Succed = true, Message = "Network interface creado" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Netwwork interface non creado: {ex.Message}" };
            }
        }
    }
}
