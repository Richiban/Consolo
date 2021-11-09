using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Richiban.Cmdr.Generators;

namespace Richiban.Cmdr.Writers
{
    internal abstract class CodeWriter
    {
        protected abstract GeneratorExecutionContext Context { get; }
        protected abstract ICodeFileGenerator CodeFileGenerator { get; }
        protected abstract string FileName { get; }

        public void WriteToContext()
        {
            var source = CodeFileGenerator.GetCode();
            
            Context.AddSource(FileName, SourceText.From(source, Encoding.UTF8));
        }
    }
}