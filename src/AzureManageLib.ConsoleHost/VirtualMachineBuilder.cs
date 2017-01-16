
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Management.Compute;
    using Microsoft.WindowsAzure.Management.Compute.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class VirtualMachineBuilder
    {
        private readonly SubscriptionCloudCredentials credentials;

        internal VirtualMachineBuilder(SubscriptionCloudCredentials credentials)
        {
            this.credentials = credentials;
        }

        internal async Task<string> CreateVirtualMachineAsync(string imagePattern, string adminName, string adminPwd, string vmName, string serviceName, string storageName, string deployName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    var imageName = (await computeClient.VirtualMachineOSImages.GetAsync(imagePattern).ConfigureAwait(false)).Name;
                    var windowsConfigSet = new ConfigurationSet
                    {
                        ConfigurationSetType = ConfigurationSetTypes.WindowsProvisioningConfiguration,
                        AdminPassword = adminPwd,
                        AdminUserName = adminName,
                        ComputerName = vmName,
                        HostName = string.Format("{0}.cloudapp.net", serviceName)
                    };
                    var networkConfigSet = new ConfigurationSet
                    {
                        ConfigurationSetType = "NetworkConfiguration",
                        InputEndpoints = new List<InputEndpoint>
                          {
                            new InputEndpoint
                            {
                              Name = "PowerShell",
                              LocalPort = 5986,
                              Protocol = "tcp",
                              Port = 5986,
                            },
                            new InputEndpoint
                            {
                              Name = "Remote Desktop",
                              LocalPort = 3389,
                              Protocol = "tcp",
                              Port = 3389,
                            }
                          }
                    };
                    var vhd = new OSVirtualHardDisk
                    {
                        SourceImageName = imageName,
                        HostCaching = VirtualHardDiskHostCaching.ReadWrite,
                        MediaLink = new Uri(string.Format(
                            "https://{0}.blob.core.windows.net/vhds/{1}.vhd",
                            storageName,
                            imageName))
                    };
                    var deploymentAttributes = new Role
                    {
                        Label = "azurelibvm",
                        RoleName = vmName,
                        RoleSize = VirtualMachineRoleSize.Small,
                        RoleType = VirtualMachineRoleType.PersistentVMRole.ToString(),
                        OSVirtualHardDisk = vhd,
                        ConfigurationSets = new List<ConfigurationSet>
                          {
                            windowsConfigSet,
                            networkConfigSet
                          },
                        ProvisionGuestAgent = true
                    };

                    var createDeploymentParameters =
                      new VirtualMachineCreateDeploymentParameters
                      {
                          Name = deployName,
                          Label = deployName,
                          DeploymentSlot = DeploymentSlot.Production,
                          Roles = new List<Role> { deploymentAttributes }
                      };
                    var deploymentResult = await computeClient.VirtualMachines.CreateDeploymentAsync(
                        serviceName,
                        createDeploymentParameters).ConfigureAwait(false);
                }

                return "Creado la virtual machine";
            }
            catch (Exception ex)
            {
                return $"Virtual machine no creada: {ex.Message}";
            }
        }

        internal async Task<StepResult> DeleteVirtualMachineAsync(string serviceName, string deployName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    string vmStatus = "Created";
                    while (!vmStatus.Equals("Running"))
                    {
                        var vmStatusResponse = await computeClient.Deployments.GetBySlotAsync(serviceName, DeploymentSlot.Production).ConfigureAwait(false);
                        vmStatus = vmStatusResponse.Status.ToString();
                    }

                    var deleteDeploymentResult = await computeClient.Deployments.DeleteByNameAsync(serviceName, deployName, true).ConfigureAwait(false);
                }

                return new StepResult() { Succed = true, Message = "Eliminado VM" };
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Virtual machine no eliminada: {ex.Message}" };
            }
        }
    }
}
