using System;

namespace Richiban.Cmdr.Tests
{
    internal class TestProgram
    {
        [CommandLine]
        public object SingleStringParameterTestAction(string paramA) =>
            new
            {
                ExecutedAction = nameof(SingleStringParameterTestAction),
                Output = paramA
            };

        [CommandLine]
        public object MultiStringParameterTestAction(string paramA, string paramB) =>
            new
            {
                ExecutedAction = nameof(MultiStringParameterTestAction),
                Output = $"{new { paramA, paramB }}"
            };

        [CommandLine, Route("test-route")]
        public object RoutedAction() => new { ExecutedAction = nameof(RoutedAction) };

        [CommandLine, Route("test-route-1", "test-route-2")]
        public object MultiLevelRoutedAction() =>
            new { ExecutedAction = nameof(MultiLevelRoutedAction) };

        /// <summary>
        ///     This is the comment for test-route-1
        /// </summary>
        [CommandLine, Route("test-route-1")]
        public object SingleLevelRoutedActionWithParameter(bool param1 = false) =>
            new
            {
                ExecutedAction = nameof(SingleLevelRoutedActionWithParameter),
                Output = param1.ToString()
            };

        /// <summary>
        ///     This is the comment for two-part-route-1
        /// </summary>
        [CommandLine, Route("two-part-route-1", "two-part-route-2")]
        public object TwoPartRouteOnly(bool param1 = false) =>
            new { ExecutedAction = nameof(TwoPartRouteOnly), Output = param1.ToString() };

        /// <summary>
        ///     Comments for five-part-route
        /// </summary>
        [CommandLine,
         Route(
             "five-part-route-1",
             "five-part-route-2",
             "five-part-route-2",
             "five-part-route-4",
             "five-part-route-5")]
        public object FivePartRoute() =>
            new { ExecutedAction = nameof(FivePartRoute), Output = "" };

        [CommandLine, Route("short-form-tests")]
        public object ShortFormTestAction(
            [ShortForm(firstShortForm: 'a')] bool paramA = false,
            [ShortForm(firstShortForm: 'b', 'z')] bool paramB = false,
            [ShortForm(
                firstShortForm: 'c',
                DisallowLongForm = true)]
            bool paramC = false) =>
            new
            {
                ExecutedAction = nameof(ShortFormTestAction),
                Output = $"{new { paramA, paramB, paramC }}"
            };

        [CommandLine, Route("multi-value-param")]
        public object MultiValueParameterAction(int[] param1) =>
            new
            {
                ExecutedAction = nameof(MultiValueParameterAction),
                Output = $"[{string.Join(", ", param1)}]"
            };

        [CommandLine, Route("multi-value-params-param")]
        public object MultiValueParamsParameterAction(
            string nonParamsParam,
            params string[] remainingParams) =>
            new
            {
                ExecutedAction = nameof(MultiValueParamsParameterAction),
                Output =
                    $"{{ nonParamsParam = {nonParamsParam}, remainingParams = [{string.Join(", ", remainingParams)}] }}"
            };

        [Route("class-test-route-1")]
        public class ClassRoutedActions
        {
            [CommandLine, Route("class-test-route-2", "class-test-route-3")]
            public object MultiLevelClassRoutedAction() =>
                new { ExecutedAction = nameof(MultiLevelClassRoutedAction) };

            [CommandLine, Route("")]
            public object ZeroLevelRoutedAction() =>
                new { ExecutedAction = nameof(ZeroLevelRoutedAction) };

            [CommandLine, Route("class-test-route-2")]
            public object
                SingleLevelClassRoutedActionWithParameter(bool param1 = false) =>
                new
                {
                    ExecutedAction =
                        nameof(SingleLevelClassRoutedActionWithParameter),
                    Output = param1.ToString()
                };
        }
    }
}