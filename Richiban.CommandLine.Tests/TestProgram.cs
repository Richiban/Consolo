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

        [CommandLine, Route("test-route-1")]
        /// <summary>
        /// This is the comment for test-route-1
        /// </summary>
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
