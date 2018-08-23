using NUnit.Framework;
using System;

namespace Richiban.CommandLine.Tests
{
    class TraceTests : CommandLineTest
    {
        [Test]
        public void StartOfAssemblyScanningIsTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = @"[Trace]: Scanning assemblies:
        Richiban.CommandLine.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void RegisteredTypeConverterInstancesAreTraced()
        {
            var traceOutput = RunTest("/?trace").TraceOutput;

            var expectedContent = @"[Trace]: Registered ITypeConverter instances:
                Richiban.CommandLine.MissingValueTypeConverter
                Richiban.CommandLine.StringPassthroughTypeConverter
                Richiban.CommandLine.EnumTypeConverter
                Richiban.CommandLine.SystemConvertibleTypeConverter
                Richiban.CommandLine.ConstructFromStringTypeConverter";

            Assert.That(traceOutput, Does.Contain(expectedContent));
        }

        [Test]
        public void MethodMappingAttemptsAreTraced()
        {
            var traceOutput = RunTest().TraceOutput;

            var expectedContent = "[Trace]: Attempting to map method Richiban.CommandLine.Tests.TestProgram.SingleStringParameterTestAction from arguments \"\"";

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
            var totalTraceOutput = String.Join(Environment.NewLine, traceOutput);

            var expectedContent = @"(\r\n|\r|\n)\[Trace\]: CommandLine sequence finished in \d\d:\d\d:\d\d\.\d+";

            Assert.That(totalTraceOutput, Does.Match(expectedContent));
        }
    }
}
