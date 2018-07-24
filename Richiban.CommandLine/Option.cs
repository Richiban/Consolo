namespace Richiban.CommandLine
{
    internal struct Option<T>
    {
        public T Value { get; }
        public bool HasValue { get; }

        public Option(T value)
        {
            Value = value;
            HasValue = value != null;
        }

        public static implicit operator Option<T>(Prelude.OptionNone none) => new Option<T>();
        public static implicit operator Option<T>(T value) => new Option<T>(value);
    }
}
