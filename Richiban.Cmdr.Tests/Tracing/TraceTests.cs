using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests
{
    internal class TraceTests : CommandLineTest
    {
        [Test]
        public void StartOfAssemblyScanningIsTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = @"[Trace]: Scanning assemblies: 
        Richiban.Cmdr.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void RegisteredTypeConverterInstancesAreTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = new[]
            {
                @"[Trace]: Registered ITypeConverter instances:",
                "        Richiban.Cmdr.MissingValueTypeConverter",
                "        Richiban.Cmdr.StringPassthroughTypeConverter",
                "        Richiban.Cmdr.EnumTypeConverter",
                "        Richiban.Cmdr.SystemConvertibleTypeConverter",
                "        Richiban.Cmdr.ConstructFromStringTypeConverter"
            };

            foreach (var expectedContent1 in expectedContent)
            {
                Assert.That(traceOutput, Does.Contain(expectedContent1));
            }
        }

        [Test]
        public void MethodMappingAttemptsAreTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent =
                "[Trace]: Attempting to map method Richiban.Cmdr.Tests.TestProgram.SingleStringParameterTestAction from arguments \"\"";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void ResultsOfMethodMappingIsTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = "[Trace]: Generated the following Mappings:";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void BestMatchFromMethodMappingsIsTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = "[Trace]: Selecting the best matches:";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void TotalExecutionTimeIsTraced()
        {
            var traceOutput = RunTest().TraceOutput;
            var totalTraceOutput = string.Join(Environment.NewLine, traceOutput);

            var expectedContent =
                @"(\r\n|\r|\n)\[Trace\]: CommandLine sequence finished in \d\d:\d\d:\d\d\.\d+";

            Assert.That(totalTraceOutput, Does.Match(expectedContent));
        }
    }
}