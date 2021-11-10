using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Generators;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Writers
{
    internal class ProgramClassWriter : CodeWriter
    {
        public ProgramClassWriter(
            GeneratorExecutionContext context,
            ImmutableArray<MethodModel> methodModels)
        {
            Context = context;
            CodeFileGenerator = new ProgramClassCodeFileGenerator(methodModels);
        }

        protected override GeneratorExecutionContext Context { get; }
        protected override ICodeFileGenerator CodeFileGenerator { get; }
        protected override string FileName => "Program.g.cs";
    }
}