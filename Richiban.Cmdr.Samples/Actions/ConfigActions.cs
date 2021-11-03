using Richiban.Cmdr;
using System;

namespace Richiban.Cmdr.Samples
{
    class ConfigActions
    {
        [CommandLine, Route("config")]
        public void GetConfigValue(string settingName, bool global = false)
        {
            Console.WriteLine($"Getting {(global ? " (global)" : "")} {settingName}");
        }

        [CommandLine, Route("config")]
        public void SetConfigValue(string settingName, string settingValue, bool global = false)
        {
            Console.WriteLine($"Setting{(global ? " (global)" : "")} {settingName} = {settingValue}");
        }
    }
}
