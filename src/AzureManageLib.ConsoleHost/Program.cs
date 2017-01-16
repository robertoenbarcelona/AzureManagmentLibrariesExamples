
namespace AzureManageLib.ConsoleHost
{
    using Microsoft.Azure;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Configuration;

    public class Program
    {
        private static StorageBuilder StorageBuilder;
        private static CloudServiceBuilder ServiceBuilder;
        private static VirtualMachineBuilder VmBuilder;

        static void Main(string[] args)
        {
            var token = GetAuthorizationHeader();
            var subscripcionId = ConfigurationManager.AppSettings["subscriptionId"];
            var credential = new TokenCloudCredentials(subscripcionId, token);
            ExecuteAPI(credential);
            Console.WriteLine("¿Cierre?");
            Console.ReadLine();
        }

        private static void ExecuteAPI(TokenCloudCredentials credential)
        {
            var proceed = CreateStorage(credential);
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            if (!proceed) { return; }
            proceed = CreateCloudService(credential);
            Console.WriteLine("¿Proseguir?");
            Console.ReadLine();
            var deleting = true;
            if (proceed)
            {
                Console.WriteLine("¿Proseguir?");
                CreateVirtuaMachine(credential);
                Console.WriteLine("¿Eliminar la VM?");
                deleting = Console.ReadLine() == "y";
                if (deleting)
                {
                    deleting = DeleteVirtuaMachine();
                }
            }

            if (deleting)
            {
                Console.WriteLine("¿Eliminar servicio y storage?");
                deleting = Console.ReadLine() == "y";
                DeleteStorage();
                DeleteService();
            }
        }

        private static bool CreateStorage(TokenCloudCredentials credential, bool isArm = false)
        {
            Console.WriteLine("Creando storage...");
            StorageBuilder = new StorageBuilder(credential);
            var name = ConfigurationManager.AppSettings["storageName"];
            if (isArm)
            {
                name += "arm";
            }

            var result = StorageBuilder.CreateStorageAccountAsync(name).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static void DeleteStorage(bool isArm = false)
        {
            Console.WriteLine("Eliminando el storage...");
            var name = ConfigurationManager.AppSettings["storageName"];
            if (isArm)
            {
                name += "arm";
            }

            var result = StorageBuilder.DeleteStorageAccountAsync(name).Result;
            Console.WriteLine(result);
        }

        private static bool CreateCloudService(TokenCloudCredentials credential, bool isArm = false)
        {
            Console.WriteLine("Creando el servicio...");
            ServiceBuilder = new CloudServiceBuilder(credential);
            var name = ConfigurationManager.AppSettings["serviceName"];
            if (isArm)
            {
                name += "arm";
            }

            var result = ServiceBuilder.CreateCloudServiceAsync(name).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
        }

        private static void DeleteService(bool isArm = false)
        {
            Console.WriteLine("Eliminando el servicio...");
            var name = ConfigurationManager.AppSettings["serviceName"];
            if (isArm)
            {
                name += "arm";
            }

            var result = ServiceBuilder.DeleteCloudServiceAsync(name).Result;
            Console.WriteLine(result);
        }

        private static void CreateVirtuaMachine(TokenCloudCredentials credential, bool isArm = false)
        {
            Console.WriteLine("Creando la virtual machine...");
            VmBuilder = new VirtualMachineBuilder(credential);
            var vmName = ConfigurationManager.AppSettings["vmName"];
            var storageName = ConfigurationManager.AppSettings["storageName"];
            var serviceName = ConfigurationManager.AppSettings["serviceName"];
            var deployName = ConfigurationManager.AppSettings["deployName"];
            if (isArm)
            {
                vmName += "arm";
                storageName += "arm";
                serviceName += "arm";
                deployName += "arm";
            }

            var result = VmBuilder.CreateVirtualMachineAsync(
                    ConfigurationManager.AppSettings["imageFilter"],
                    ConfigurationManager.AppSettings["adminName"],
                    ConfigurationManager.AppSettings["adminPassword"],
                    vmName,
                    serviceName,
                    storageName,
                    deployName)
                    .Result;
            Console.WriteLine(result);
        }

        private static bool DeleteVirtuaMachine(bool isArm = false)
        {
            Console.WriteLine("Eliminando la VM...");
            var deployName = ConfigurationManager.AppSettings["deployName"];
            var serviceName = ConfigurationManager.AppSettings["serviceName"];
            if (isArm)
            {
                deployName += "arm";
                serviceName += "arm";
            }
            var result = VmBuilder.DeleteVirtualMachineAsync(serviceName, deployName).Result;
            Console.WriteLine(result.Message);
            return result.Succed;
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
//ClientCredential cc = new ClientCredential(ClientId, ServicePrincipalPassword);
//var context = new AuthenticationContext(string.Format(ConfigurationManager.AppSettings["login"], ConfigurationManager.AppSettings["tenantId"]));
//var result = context.AcquireTokenAsync(ConfigurationManager.AppSettings["apiEndpoint"], cc);