using System;
using System.Collections.Generic;
using System.Text;

namespace Richiban.CommandLine.Tests
{
    class TestProgram
    {
        public static string ExecutedAction;
        public static string Output;

        [CommandLine]
        public void SingleStringParameterTestAction(string paramA)
        {
            ExecutedAction = nameof(SingleStringParameterTestAction);
            Output = paramA;
        }

        [CommandLine]
        public void MultiStringParameterTestAction(string paramA, string paramB)
        {
            ExecutedAction = nameof(MultiStringParameterTestAction);
            Output = $"{new { paramA, paramB }}";
        }

        [CommandLine, Route("test-route")]
        public void RoutedAction()
        {
            ExecutedAction = nameof(RoutedAction);
        }

        [CommandLine, Route("test-route-1", "test-route-2")]
        public void MultiLevelRoutedAction()
        {
            ExecutedAction = nameof(MultiLevelRoutedAction);
        }

        [CommandLine, Route("test-route-1")]
        public void singleLevelRoutedActionWithParameter(bool param1)
        {
            ExecutedAction = nameof(singleLevelRoutedActionWithParameter);
            Output = param1.ToString();
        }
    }
}
