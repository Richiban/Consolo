using System;

namespace Richiban.Cmdr
{
    class MethodMatchPriority : IComparable<MethodMatchPriority>
    {
        public MethodMatchPriority(
            bool usesOptionalParameters, 
            bool usesPositionalParameters,
            bool explicitRouteMatch)
        {
            UsesOptionalParameters = usesOptionalParameters;
            UsesPositionalParameters = usesPositionalParameters;
            ExplicitRouteMatch = explicitRouteMatch;
        }

        public bool UsesOptionalParameters { get; }
        public bool UsesPositionalParameters { get; }
        public bool ExplicitRouteMatch { get; }

        public int CompareTo(MethodMatchPriority other)
        {
            return (ExplicitRouteMatch, !UsesOptionalParameters, !UsesPositionalParameters)
                .CompareTo((other.ExplicitRouteMatch, !other.UsesOptionalParameters, !other.UsesPositionalParameters));
        }
    }
}