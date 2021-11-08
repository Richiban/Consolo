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
        protected abstract ICodeGenerator CodeGenerator { get; }
        protected abstract string FileName { get; }

        public void WriteToContext()
        {
            var codeLines = CodeGenerator.GetCodeLines();

            var sb = new StringBuilder();

            foreach (var line in codeLines)
            {
                sb.AppendLine(line);
            }

            Context.AddSource(FileName, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}