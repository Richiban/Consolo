using Richiban.CommandLine;

namespace ConsoleApp1
{
    class GitConfigCommandLineAction
    {
        [CommandLine, Verb("config")]
        public void GetConfigValue(string settingName, bool global = false)
        {
            $"Getting {(global ? " (global)" : "")} {settingName}".Dump();
        }

        [CommandLine, Verb("config")]
        public void SetConfigValue(string settingName, string settingValue, bool global = false)
        {
            $"Setting {(global ? " (global)" : "")} {settingName} = {settingValue}".Dump();
        }
    }
}
