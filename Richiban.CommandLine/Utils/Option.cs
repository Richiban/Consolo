using System;
using System.Collections.Generic;

namespace Richiban.CommandLine
{
    internal struct Option<T>
    {
        private readonly T _value;

        public Option(T value)
        {
            _value = value;
            HasValue = value != null;
        }

        public bool HasValue { get; }

        public static implicit operator Option<T>(T value) => new Option<T>(value);

        public static implicit operator Option<T>(Prelude.OptionNone _) => new Option<T>();

        internal void Match(Action None, Action<T> Some)
        {
            if (HasValue)
                Some(_value);
            else
                None();
        }

        public Option<R> IfSome<R>(Func<T, R> f)
        {
            if (HasValue)
                return f(_value);

            return default;
        }

        public void IfSome(Action<T> f)
        {
            if (HasValue)
                f(_value);
        }

        public static IEnumerable<T> Choose(IEnumerable<Option<T>> source)
        {
            foreach (var item in source)
                if (item.HasValue)
                    yield return item._value;
        }
    }
}
