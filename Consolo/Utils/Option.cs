using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Consolo.Prelude;

namespace Consolo;

readonly struct Option<T>(T? value)
{
    public bool HasValue { get; } = value is not null;
    private T? Value => value;

    public bool IsNone => !HasValue;

    internal bool IsSome(out T v)
    {
        v = value!;

        return HasValue;
    }

    public void Deconstruct(out bool hasValue, out T? value)
    {
        hasValue = HasValue;
        value = Value;
    }

    internal Option<R> Map<R>(Func<T, R> mapper)
    {
        if (HasValue)
        {
            return mapper(value!);
        }

        return new();
    }

    internal Option<R> FlatMap<R>(Func<T, Option<R>> mapper)
    {
        if (HasValue)
        {
            return mapper(value!);
        }

        return new();
    }

    public T GetValueOrDefault(T defaultValue)
    {
        if (HasValue)
        {
            return value!;
        }

        return defaultValue;
    }

    public T GetValueOrDefault(Func<T> getDefaultValue)
    {
        if (HasValue)
        {
            return value!;
        }

        return getDefaultValue();
    }

    public static T operator |(Option<T> option, T defaultValue) => 
        option.GetValueOrDefault(defaultValue);

    public static Option<T> operator |(Option<T> left, Option<T> right) => 
        left.HasValue ? left : right;

    public static bool operator true(Option<T> option) => 
        option.HasValue;
        
    public static bool operator false(Option<T> option) => 
        !option.HasValue;

    public static implicit operator Option<T>(T? value) => new(value);
    public static implicit operator Option<T>(OptionNone _) => new();

    public static bool operator == (Option<T> option, T value) => 
        option.HasValue && option.Value?.Equals(value) == true;

    public static bool operator != (Option<T> option, T value) => 
        !(option == value);

    public bool Equals(Option<T> other) => 
        HasValue == other.HasValue && 
        (!HasValue || value?.Equals(other.Value) == true);

    public override bool Equals(object? obj) =>
        obj is Option<T> other && Equals(other);

    public override int GetHashCode() =>
        HasValue ? value?.GetHashCode() ?? 0 : 0;

    public override string ToString() =>
        HasValue ? value!.ToString() : "";
}