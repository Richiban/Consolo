using System;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Generators;

namespace Richiban.Cmdr.Writers
{
    internal class CmdrAttributeWriter : CodeWriter
    {
        private readonly CmdrAttribute _cmdrAttribute;

        public CmdrAttributeWriter(
            CmdrAttribute cmdrAttribute,
            GeneratorExecutionContext context)
        {
            _cmdrAttribute = cmdrAttribute;
            Context = context;
            CodeFileGenerator = GetCodeGenerator();
        }

        private CmdrAttributeFileGenerator GetCodeGenerator() =>
            new CmdrAttributeFileGenerator(_cmdrAttribute);

        protected override GeneratorExecutionContext Context { get; }
        protected override ICodeFileGenerator CodeFileGenerator { get; }
        protected override string FileName => "CmdrMethodAttribute.g.cs";
    }
}