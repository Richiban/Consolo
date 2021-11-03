using System;

namespace Richiban.CommandLine.Tests.TypeConversion
{
    [Route("type-conversion-tests")]
    class TypeConversionTestProgram
    {
        [CommandLine, Route("string")]
        public object StringTypeConversionAction(string param) => new
        {
            ExecutedAction = nameof(StringTypeConversionAction),
            Output = $"{new { param }}"
        };

        [CommandLine, Route("enum")]
        public object EnumTypeConversionAction(TestEnum param) => new
        {
            ExecutedAction = nameof(EnumTypeConversionAction),
            Output = $"{new { param }}"
        };

        [CommandLine, Route("int")]
        public object IntTypeConversionAction(int param) => new
        {
            ExecutedAction = nameof(IntTypeConversionAction),
            Output = $"{new { param }}"
        };

        [CommandLine, Route("uri")]
        public object UriTypeConversionAction(Uri param) => new
        {
            ExecutedAction = nameof(UriTypeConversionAction),
            Output = $"{new { param }}"
        };

        [CommandLine, Route("missing")]
        public object MissingTypeConversionAction(int param = 1) => new
        {
            ExecutedAction = nameof(MissingTypeConversionAction),
            Output = $"{new { param }}"
        };

        public enum TestEnum
        {
            MemberA, MemberB
        }
    }
}
