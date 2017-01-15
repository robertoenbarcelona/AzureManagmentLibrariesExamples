
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Configuration;
    using System.Threading.Tasks;

    public class Program
    {
        private static StorageBuilder StorageBuilder;
        private static CloudServiceBuilder ServiceBuilder;
        private static VirtualMachineBuilder VmBuilder;

        static void Main(string[] args)
        {

            var token = GetAuthorizationHeader();
            var credential = new TokenCloudCredentials(ConfigurationManager.AppSettings["subscriptionId"], token);
            var proceed = CreateStorage(credential);
            var deleting = false;
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            if (proceed)
            {
                proceed = CreateCloudService(credential);
                Console.WriteLine("¿Proseguir?");
                Console.ReadLine();
            }
            if (proceed)
            {
                Console.WriteLine("¿Proseguir?");
                CreateVirtuaMachine(credential);
            }

            Console.WriteLine("¿Eliminar la VM?");
            deleting = Console.ReadLine() == "y";
            proceed = DeleteVirtuaMachine();
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            if (proceed)
            {
                DeleteStorage();
                DeleteService();
            }

            Console.WriteLine("¿Cierre?");
            Console.ReadLine();
        }

        private static void CreateVirtuaMachine(TokenCloudCredentials credential)
        {
            Console.WriteLine("Creando la virtual machine...");
            VmBuilder = new VirtualMachineBuilder(credential);
            var result = VmBuilder.CreateVirtualMachineAsync(
                    ConfigurationManager.AppSettings["imageFilter"],
                    ConfigurationManager.AppSettings["adminName"],
                    ConfigurationManager.AppSettings["adminPassword"],
                    ConfigurationManager.AppSettings["vmName"],
                    ConfigurationManager.AppSettings["serviceName"],
                    ConfigurationManager.AppSettings["storageName"],
                    ConfigurationManager.AppSettings["deployName"])
                    .Result;
            Console.WriteLine(result);
        }

        private static bool DeleteVirtuaMachine()
        {
            Console.WriteLine("Eliminando la VM...");
            var result = VmBuilder.DeleteVirtualMachineAsync(ConfigurationManager.AppSettings["serviceName"], ConfigurationManager.AppSettings["deployName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static bool CreateStorage(TokenCloudCredentials credential)
        {
            Console.WriteLine("Creando storage...");
            StorageBuilder = new StorageBuilder(credential);
            var result = StorageBuilder.CreateStorageAccountAsync(ConfigurationManager.AppSettings["storageName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static void DeleteStorage()
        {
            Console.WriteLine("Eliminando el storage...");
            var result = StorageBuilder.DeleteStorageAccountAsync(ConfigurationManager.AppSettings["storageName"]).Result;
            Console.WriteLine(result);
        }

        private static bool CreateCloudService(TokenCloudCredentials credential)
        {
            Console.WriteLine("Creando el servicio...");
            ServiceBuilder = new CloudServiceBuilder(credential);
            var result = ServiceBuilder.CreateCloudServiceAsync(ConfigurationManager.AppSettings["serviceName"]).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static void DeleteService()
        {
            Console.WriteLine("Eliminando el servicio...");
            var result = ServiceBuilder.DeleteCloudServiceAsync(ConfigurationManager.AppSettings["serviceName"]).Result;
            Console.WriteLine(result);
        }

        private static string GetAuthorizationHeader()
        {
            AuthenticationResult result = null;

            var context = new AuthenticationContext(string.Format(
              ConfigurationManager.AppSettings["login"],
              ConfigurationManager.AppSettings["tenantId"]));

            result = context.AcquireTokenAsync(
              ConfigurationManager.AppSettings["apiEndpoint"],
              ConfigurationManager.AppSettings["appId"],
              new Uri(ConfigurationManager.AppSettings["redirectUri"]),
              new PlatformParameters(PromptBehavior.Auto, null)).Result;

            return result.AccessToken;
        }
    }
}
//var credential = new ClientCredential(clientId: < your ApplicationId >, clientSecret: < your app password >);
//var result = authenticationContext.AcquireToken(resource: "https://management.azure.com/", clientCredential: credential);
