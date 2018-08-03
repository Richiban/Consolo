using System;

namespace TracerAttributes
{
    [AttributeUsage(
        AttributeTargets.Class 
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Constructor,
        AllowMultiple = true, 
        Inherited = true)]
    class TraceOn : Attribute
    {
        public TraceTarget Target { get; set; }
        public TraceOn() { }
        public TraceOn(TraceTarget traceTarget) { Target = traceTarget; }
    }
}