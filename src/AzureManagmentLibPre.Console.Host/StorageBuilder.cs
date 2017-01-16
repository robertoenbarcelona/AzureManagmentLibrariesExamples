
namespace AzureManagmentLibPre.ConsoleHost
{
    using Microsoft.Azure.Management.Storage;
    using Microsoft.Azure.Management.Storage.Models;
    using Microsoft.Rest;
    using Microsoft.WindowsAzure.Management.Models;
    using System;
    using System.Threading.Tasks;


    internal class StorageBuilder
    {
        private readonly TokenCredentials credentials;
        private readonly string subscriptionId;

        internal StorageBuilder(TokenCredentials credentials, string subscriptionId)
        {
            this.credentials = credentials;
            this.subscriptionId = subscriptionId;
        }

        internal async Task<StepResult> CreateStorageAccountAsync(string storageName, string resourceName)
        {
            try
            {
                using (var storageClient = new StorageManagementClient(this.credentials) { SubscriptionId = this.subscriptionId })
                {
                    var free = await storageClient.StorageAccounts.CheckNameAvailabilityAsync(storageName).ConfigureAwait(false);
                    if (free.NameAvailable.HasValue && free.NameAvailable.Value)
                    {
                        await storageClient.StorageAccounts.CreateAsync(
                            resourceName,
                            storageName,
                            new StorageAccountCreateParameters
                            {
                                Sku = new Sku() { Name = SkuName.StandardLRS },
                                Kind = Kind.Storage,
                                Location = LocationNames.WestEurope
                            }).ConfigureAwait(false);
                        return new StepResult() { Succed = true, Message = "Storage creado" };
                    }

                    Console.WriteLine("ResourceGroup ya existente. ¿Proseguir con este?");
                    var result = Console.ReadLine() == "y";
                    return new StepResult() { Succed = result, Message = "Nombre ya utilizado" };
                }
            }
            catch (Exception ex)
            {
                return new StepResult() { Succed = false, Message = $"Storage non creado: {ex.Message}" };
            }
        }
    }
}
