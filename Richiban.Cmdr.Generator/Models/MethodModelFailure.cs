using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr.Models
{
    internal class MethodModelFailure
    {
        public MethodModelFailure(string message, Location? location)
        {
            Message = message;
            Location = location;
        }

        public string Message { get; }
        public Location? Location { get; }
    }
}