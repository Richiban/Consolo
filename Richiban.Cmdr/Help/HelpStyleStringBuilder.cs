using System;
using System.Linq;
using System.Text;

namespace Richiban.Cmdr
{
    internal class HelpStyleStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private string _indentation;

        private int _indentationLevel;
        private bool _isAtStartOfLine = true;

        public HelpStyleStringBuilder()
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
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            if (_isAtStartOfLine)
            {
                _stringBuilder.Append(_indentation);
            }

            _stringBuilder.Append(s);
            _isAtStartOfLine = false;
        }

        public override string ToString() => _stringBuilder.ToString();

        private void GenerateIndentation() =>
            _indentation = string.Concat(Enumerable.Repeat(" ", _indentationLevel * 4));

        private class HelpStringBuilderIndenter : IDisposable
        {
            private readonly HelpStyleStringBuilder _helpStringBuilder;

            public HelpStringBuilderIndenter(HelpStyleStringBuilder helpStringBuilder)
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
    }
}