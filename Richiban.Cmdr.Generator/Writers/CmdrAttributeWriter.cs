using System;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Generators;

namespace Richiban.Cmdr.Writers
{
    internal class CmdrAttributeWriter : CodeWriter
    {
        private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;

        public CmdrAttributeWriter(
            CmdrAttributeDefinition cmdrAttributeDefinition,
            GeneratorExecutionContext context)
        {
            _cmdrAttributeDefinition = cmdrAttributeDefinition;
            Context = context;
            CodeFileGenerator = GetCodeGenerator();
        }

        private CmdrAttributeFileGenerator GetCodeGenerator() =>
            new CmdrAttributeFileGenerator(_cmdrAttributeDefinition);

        protected override GeneratorExecutionContext Context { get; }
        protected override ICodeFileGenerator CodeFileGenerator { get; }
        protected override string FileName => $"{_cmdrAttributeDefinition.LongName}.g.cs";
    }
}