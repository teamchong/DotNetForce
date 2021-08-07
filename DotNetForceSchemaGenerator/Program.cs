using DotNetForce;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetForceSchemaGenerator
{
    public class Program
    {
        private static async Task Main()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            await RunTransformAsync().ConfigureAwait(false);
        }

        private static string Prompt(string message, string defaultValue = "")
        {
            Console.Write(message);
            if (!string.IsNullOrEmpty(defaultValue))
                Console.Write($" ({defaultValue})");
            Console.Write(": ");
            var value = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value == "" ? null : value;
        }

        private static async Task RunTransformAsync()
        {
            var output = new StringBuilder();
            var loginUrl = "";
            var clientId = "";
            var clientSecret = "";
            var userName = "";
            var password = "";

            var schemaNamespace = "";
            var schemaName = "";
            if (loginUrl == "") loginUrl = "https://test.salesforce.com";
            if (schemaNamespace == "") schemaNamespace = "DotNetForce.Schemas";
            if (schemaName == "") schemaName = "Uat";
            try
            {
                loginUrl = Prompt("Login URL?", loginUrl) ?? throw new ArgumentNullException(nameof(loginUrl));
                clientId = Prompt("Client Id?", clientId) ?? throw new ArgumentNullException(nameof(clientId));
                clientSecret = Prompt("Client Secret?", clientSecret) ?? throw new ArgumentNullException(nameof(clientSecret));
                userName = Prompt("User Name?", userName) ?? throw new ArgumentNullException(nameof(userName));
                password = Prompt("Password?", password) ?? throw new ArgumentNullException(nameof(password));
                schemaNamespace = Prompt("Namespace?", schemaNamespace) ?? throw new ArgumentNullException(nameof(schemaNamespace));
                schemaName = Prompt("File Name?", schemaName) ?? throw new ArgumentNullException(nameof(schemaName));
                var loginUri = new Uri($"{loginUrl}/services/oauth2/token");
                var client = await DnfClient.LoginAsync(loginUri, clientId, clientSecret, userName, password, Console.WriteLine)
                    .ConfigureAwait(false);

                var generator = new SchemaGenerator(output, schemaNamespace, schemaName);
                await generator.GenerateAsync(client).ConfigureAwait(false);



            }
            finally
            {
                var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, $"{schemaName}.cs");
                await File.WriteAllTextAsync(filePath, output.ToString());
            }
        }
    }
}
