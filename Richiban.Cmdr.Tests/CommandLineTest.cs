using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.Cmdr.Tests
{
    [TestFixture]
    abstract class CommandLineTest
    {
        protected readonly CommandLineConfiguration _configuration = CommandLineConfiguration.GetDefault();

        protected Result RunTest(params string[] args)
        {
            _configuration.AssembliesToScan = new List<Assembly> { GetType().Assembly };

            var helpOutput = (string)null;
            var traceOutput = new List<string>();

            _configuration.HelpOutput = help => helpOutput = help;
            _configuration.TraceOutput = s => traceOutput.Add(s);

            var programOutput = CommandLine.Execute(_configuration, args);

            return new Result(helpOutput, programOutput, traceOutput);
        }

        public class Result
        {
            public Result(string helpOutput, dynamic programOutput, IReadOnlyList<string> traceOutput)
            {
                OutputHelp = helpOutput;
                ProgramOutput = programOutput;
                TraceOutput = traceOutput;
            }

            public string OutputHelp { get; }
            public dynamic ProgramOutput { get; }
            public IReadOnlyList<string> TraceOutput { get; }
        }
    }
}
