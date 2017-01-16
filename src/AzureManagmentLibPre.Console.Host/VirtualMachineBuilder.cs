
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure.Management.Compute;
    using Microsoft.Azure.Management.Compute.Models;
    using Microsoft.Azure.Management.Network;
    using Microsoft.Rest;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class VirtualMachineBuilder
    {
        private readonly TokenCredentials credentials;
        private readonly string subscriptionId;

        public VirtualMachineBuilder(TokenCredentials credentials, string subscriptionId)
        {
            this.credentials = credentials;
            this.subscriptionId = subscriptionId;
        }

        internal async Task<StepResult> CreateVirtualMachineAsync(string adminName, string adminPwd, string vmName, string storageName, string nicName, string avsetName, string groupName)
        {
            Console.WriteLine("Creando la virtual machine...");
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials) { SubscriptionId = this.subscriptionId })
                {
                    using (var networkManagementClient = new NetworkManagementClient(this.credentials) { SubscriptionId = subscriptionId })
                    {
                        var nic = networkManagementClient.NetworkInterfaces.Get(groupName, nicName);
                        using (var computeManagementClient = new ComputeManagementClient(this.credentials) { SubscriptionId = subscriptionId })
                        {
                            var avSet = computeManagementClient.AvailabilitySets.Get(groupName, avsetName);
                            var res = await computeManagementClient.VirtualMachines.CreateOrUpdateAsync(
                                        groupName,
                                        vmName,
                                        new VirtualMachine
                                        {
                                            Location = LocationNames.WestEurope,
                                            AvailabilitySet = new SubResource
                                            {
                                                Id = avSet.Id
                                            },
                                            HardwareProfile = new HardwareProfile
                                            {
                                                VmSize = "Standard_A0"
                                            },
                                            OsProfile = new OSProfile
                                            {
                                                AdminUsername = adminName,
                                                AdminPassword = adminPwd,
                                                ComputerName = vmName,
                                                WindowsConfiguration = new WindowsConfiguration
                                                {
                                                    ProvisionVMAgent = true
                                                }
                                            },
                                            NetworkProfile = new NetworkProfile
                                            {
                                                NetworkInterfaces = new List<NetworkInterfaceReference> { new NetworkInterfaceReference { Id = nic.Id } }
                                            },
                                            StorageProfile = new StorageProfile
                                            {
                                                ImageReference = new ImageReference
                                                {
                                                    Publisher = "MicrosoftWindowsServer",
                                                    Offer = "WindowsServer",
                                                    Sku = "2012-R2-Datacenter",
                                                    Version = "latest"
                                                },
                                                OsDisk = new OSDisk
                                                {
                                                    Name = "myazlintestdisk",
                                                    CreateOption = DiskCreateOptionTypes.FromImage,
                                                    Vhd = new VirtualHardDisk
                                                    {
                                                        Uri = "http://" + storageName + ".blob.core.windows.net/vhds/myazlintestdisk.vhd"
                                                    }
                                                }
                                            }
                                        }
                            ).ConfigureAwait(false);
                        }
                    }

                    return new StepResult() { Succed = true, Message = "VM creada" };
                }
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Virtual machine no creada: {ex.Message}" };
            }
        }
    }
}
