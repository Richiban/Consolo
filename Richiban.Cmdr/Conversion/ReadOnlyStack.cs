using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Richiban.Cmdr
{
    abstract class ImmutableStack<T>
    {
        private ImmutableStack() { }

        public abstract (T, ImmutableStack<T>) Pop();
        public abstract T Peek();
        public abstract bool IsEmpty { get; }
        public abstract int Count { get; }

        public static ImmutableStack<T> Empty { get; } = new EmptyStack();

        public static implicit operator ImmutableStack<T>(Prelude.OptionNone none) =>
            Empty;

        public static ImmutableStack<T> CopyFrom(Stack<T> stack)
        {
            var newStack = (ImmutableStack<T>)new EmptyStack();

            foreach(var item in stack)
            {
                newStack = new HeadAndTail(item, newStack);
            }

            return newStack;
        }

        class HeadAndTail : ImmutableStack<T>
        {
            private readonly T _head;
            private readonly ImmutableStack<T> _tail;

            public HeadAndTail(T head, ImmutableStack<T> tail) =>
                (_head, _tail, Count) = (head, tail, tail.Count + 1);

            public override int Count { get; }
            public override bool IsEmpty => false;
            public override T Peek() => _head;
            public override (T, ImmutableStack<T>) Pop() => (_head, _tail);
        }

        class EmptyStack : ImmutableStack<T>
        {
            public override int Count => 0;
            public override bool IsEmpty => true;
            public override T Peek() => throw ThrowException();
            public override (T, ImmutableStack<T>) Pop() => throw ThrowException();
            private static Exception ThrowException(
                [CallerMemberName] string methodName = null) => 
                throw new InvalidOperationException($"Cannot {methodName} an empty stack");
        }
    }
}