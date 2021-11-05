using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Richiban.Cmdr.Writers
{
    abstract class CodeWriter
    {
        private readonly string _fileName;
        private readonly GeneratorExecutionContext _context;

        protected CodeWriter(string fileName, GeneratorExecutionContext context)
        {
            _fileName = fileName;
            _context = context;
        }

        protected abstract string GetCode();

        public void WriteToContext()
        {
            _context.AddSource(_fileName, SourceText.From(GetCode(), Encoding.UTF8));
        }
    }
}