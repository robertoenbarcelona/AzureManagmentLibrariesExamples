
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Management.Compute;
    using Microsoft.WindowsAzure.Management.Models;
    using Microsoft.WindowsAzure.Management.Storage;
    using Microsoft.WindowsAzure.Management.Storage.Models;
    using System;
    using System.Threading.Tasks;


    internal class StorageBuilder
    {
        private readonly SubscriptionCloudCredentials credentials;

        internal StorageBuilder(SubscriptionCloudCredentials credentials)
        {
            this.credentials = credentials;
        }

        internal async Task<StepResult> CreateStorageAccountAsync(string storageName)
        {
            try
            {
                using (var storageClient = new StorageManagementClient(this.credentials))
                {
                    var free = await storageClient.StorageAccounts.CheckNameAvailabilityAsync(storageName).ConfigureAwait(false);
                    if (free.IsAvailable)
                    {
                        await storageClient.StorageAccounts.CreateAsync(
                            new StorageAccountCreateParameters
                            {
                                Label = "azurelibstorage",
                                AccountType = "Standard_LRS",
                                Location = LocationNames.WestEurope,
                                Name = storageName
                            }).ConfigureAwait(false);
                        return new StepResult() { Succed = true, Message = "Storage creado" };
                    }
                    else
                    {
                        Console.WriteLine("Storage ya existente. ¿Proseguir con este?");
                        var result = Console.ReadLine() == "y";
                        return new StepResult() { Succed = result, Message = "Nombre ya utilizado" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Storage non creado: {ex.Message}" };
            }
        }

        internal async Task<string> DeleteStorageAccountAsync(string storageName)
        {
            try
            {
                using (var computeClient = new ComputeManagementClient(this.credentials))
                {
                    var diskCount = 1;
                    while (diskCount > 0)
                    {
                        var diskListResult = await computeClient.VirtualMachineDisks.ListDisksAsync().ConfigureAwait(false);
                        diskCount = diskListResult.Disks.Count;
                        await Task.Delay(3000).ConfigureAwait(false);
                    }
                }

                using (var storageClient = new StorageManagementClient(credentials))
                {
                    await storageClient.StorageAccounts.DeleteAsync(storageName).ConfigureAwait(false);
                }

                return "Eliminado el storage";

            }
            catch (Exception ex)
            {
                return $"Storage no eliminado: {ex.Message}";
            }
        }
    }
}
