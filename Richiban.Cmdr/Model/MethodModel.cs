using AutoLazy;
using System;
using System.Reflection;

namespace Richiban.Cmdr
{
    internal class MethodModel
    {
        private readonly MethodInfo _methodInfo;

        public MethodModel(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;

            DeclaringType = methodInfo.DeclaringType;
            InvokeFunc = methodInfo.Invoke;
            Routes = new RouteCollection(methodInfo);
            Parameters = new ParameterModelList(methodInfo.GetParameters());
            Name = methodInfo.Name;
            IsStatic = methodInfo.IsStatic;
        }

        public ParameterModelList Parameters { get; }
        public RouteCollection Routes { get; }
        public bool IsStatic { get; }

        public Type DeclaringType { get; }
        public Func<object, object[], object> InvokeFunc { get; }
        public string Name { get; }

        public int GetPartialMatchAccuracy(CommandLineArgumentList commandLineArgs) =>
            Routes.GetPartialMatchAccuracy(commandLineArgs);

        [Lazy]
        public override string ToString() => $"{_methodInfo.DeclaringType}.{_methodInfo.Name}";
    }
}