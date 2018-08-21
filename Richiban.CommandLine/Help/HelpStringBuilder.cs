using System;
using System.Linq;
using System.Text;

namespace Richiban.CommandLine
{
    class HelpStringBuilder
    {
        private readonly StringBuilder _stringBuilder;

        private int _indentationLevel;
        private string _indentation;
        private bool _isAtStartOfLine = true;

        public HelpStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public IDisposable Indent() => new HelpStringBuilderIndenter(this);

        public void AppendLine(string s)
        {
            Append(s);
            AppendLine();
        }

        public void AppendLine()
        {
            _stringBuilder.AppendLine();
            _isAtStartOfLine = true;
        }

        public void Append(string s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            if(_isAtStartOfLine)
                _stringBuilder.Append(_indentation);

            _stringBuilder.Append(s);
            _isAtStartOfLine = false;
        }

        public override string ToString() => _stringBuilder.ToString();

        private class HelpStringBuilderIndenter : IDisposable
        {
            private readonly HelpStringBuilder _helpStringBuilder;

            public HelpStringBuilderIndenter(HelpStringBuilder helpStringBuilder)
            {
                _helpStringBuilder = helpStringBuilder;
                _helpStringBuilder._indentationLevel++;
                _helpStringBuilder.GenerateIndentation();
            }

            public void Dispose()
            {
                _helpStringBuilder._indentationLevel--;
                _helpStringBuilder.GenerateIndentation();
            }
        }

        private void GenerateIndentation() =>
            _indentation = String.Concat(Enumerable.Repeat(" ", _indentationLevel * 4));
    }
}
