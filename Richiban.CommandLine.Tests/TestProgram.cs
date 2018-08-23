namespace Richiban.CommandLine.Tests
{
    class TestProgram
    {
        [CommandLine]
        public object SingleStringParameterTestAction(string paramA)
        {
            return new
            {
                ExecutedAction = nameof(SingleStringParameterTestAction),
                Output = paramA
            };
        }

        [CommandLine]
        public object MultiStringParameterTestAction(string paramA, string paramB)
        {
            return new
            {
                ExecutedAction = nameof(MultiStringParameterTestAction),
                Output = $"{new { paramA, paramB }}"
            };
        }

        [CommandLine, Route("test-route")]
        public object RoutedAction()
        {
            return new
            {
                ExecutedAction = nameof(RoutedAction)
            };
        }

        [CommandLine, Route("test-route-1", "test-route-2")]
        public object MultiLevelRoutedAction()
        {
            return new
            {
                ExecutedAction = nameof(MultiLevelRoutedAction)
            };
        }

        /// <summary>
        /// This is the comment for test-route-1
        /// </summary>
        [CommandLine, Route("test-route-1")]
        public object SingleLevelRoutedActionWithParameter(bool param1 = false)
        {
            return new
            {
                ExecutedAction = nameof(SingleLevelRoutedActionWithParameter),
                Output = param1.ToString()
            };
        }

        /// <summary>
        /// This is the comment for two-part-route-1
        /// </summary>
        [CommandLine, Route("two-part-route-1", "two-part-route-2")]
        public object TwoPartRouteOnly(bool param1 = false)
        {
            return new
            {
                ExecutedAction = nameof(TwoPartRouteOnly),
                Output = param1.ToString()
            };
        }

        /// <summary>
        /// Comments for five-part-route
        /// </summary>
        [CommandLine, Route(
            "five-part-route-1",
            "five-part-route-2",
            "five-part-route-2",
            "five-part-route-4", 
            "five-part-route-5")]
        public object FivePartRoute()
        {
            return new
            {
                ExecutedAction = nameof(FivePartRoute),
                Output = ""
            };
        }

        [CommandLine, Route("short-form-tests")]
        public object ShortFormTestAction(
            [ShortForm('a')] bool paramA = false,
            [ShortForm('b', 'z')] bool paramB = false,
            [ShortForm('c', DisallowLongForm = true)] bool paramC = false)
        {
            return new
            {
                ExecutedAction = nameof(ShortFormTestAction),
                Output = $"{new { paramA, paramB, paramC }}"
            };
        }

        [CommandLine, Route("multi-value-param")]
        public object MultiValueParameterAction(int[] param1)
        {
            return new
            {
                ExecutedAction = nameof(MultiValueParameterAction),
                Output = $"[{string.Join(", ", param1)}]"
            };
        }

        [Route("class-test-route-1")]
        public class ClassRoutedActions
        {
            [CommandLine, Route("class-test-route-2", "class-test-route-3")]
            public object MultiLevelClassRoutedAction()
            {
                return new
                {
                    ExecutedAction = nameof(MultiLevelClassRoutedAction)
                };
            }

            [CommandLine, Route("")]
            public object ZeroLevelRoutedAction()
            {
                return new
                {
                    ExecutedAction = nameof(ZeroLevelRoutedAction)
                };
            }

            [CommandLine, Route("class-test-route-2")]
            public object SingleLevelClassRoutedActionWithParameter(bool param1 = false)
            {
                return new
                {
                    ExecutedAction = nameof(SingleLevelClassRoutedActionWithParameter),
                    Output = param1.ToString()
                };
            }
        }
    }
}
