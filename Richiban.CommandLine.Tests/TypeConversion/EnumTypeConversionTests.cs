﻿using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Richiban.CommandLine.Tests.TypeConversion
{
    [TestFixture]
    class EnumTypeConversionTests : CommandLineTest
    {
        [TestCase("memberA")]
        [TestCase("memberB")]
        public void EnumTypeConversionTest(string testParam)
        {
            var result = RunTest(
                "type-conversion-tests",
                "enum",
                "-param",
                testParam);

            Assert.That(result.ExecutedAction, Is.EqualTo("EnumTypeConversionAction"));
            StringAssert.AreEqualIgnoringCase(result.Output, $"{{ param = {testParam} }}");
        }

        [TestCase("x")]
        [TestCase("")]
        public void EnumTypeConversionFailureTest(string testValue)
        {
            var ex = Assert.Throws<TypeConversionException>(() =>
                RunTest(
                    "type-conversion-tests",
                    "enum",
                    "-param",
                    testValue));

            var expectedMessage =
                $"No ITypeConverter could be found that could convert '{testValue}' to type 'TestEnum'";

            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }
    }
}
