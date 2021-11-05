using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Transformers
{
    class CommandModelTransformer
    {
        public CommandModel Transform(IEnumerable<MethodModel> methodModels)
        {
            var tree = Group(methodModels);
        }

        Tree Group(IEnumerable<MethodModel> cs)
        {
            var root = new Tree("");

            foreach (var c in cs)
            {
                root[new ListWalker<string>(c.ParentNames)] = c.NameOut;
            }

            return root;
        }

        private struct ListWalker<T>
        {
            public ListWalker(IReadOnlyList<T> list)
            {
                List = list;
                Index = 0;
            }

            private ListWalker(IReadOnlyList<T> list, int index)
            {
                List = list;
                Index = index;
            }

            public int Index { get; }
            public IReadOnlyList<T> List { get; }
            public T Current => List[Index];
            public ListWalker<T> Next => new(List, Index + 1);
            public bool AtEnd => Index >= List.Count;

            public void Deconstruct(out T current, out ListWalker<T> next)
            {
                current = Current;
                next = Next;
            }
        }

        private class Tree
        {
            public Tree(string value) => Value = value;

            public string Value { get; }

            public List<Tree> SubTrees { get; } = new();

            public string this[ListWalker<string> path]
            {
                set
                {
                    if (path.AtEnd)
                    {
                        SubTrees.Add(new Tree(value));

                        return;
                    }

                    var (current, remaining) = path;
                    var subTreeWasMatched = false;

                    foreach (var tree in SubTrees.Where(t => t.Value == current))
                    {
                        subTreeWasMatched = true;

                        tree[remaining] = value;
                    }

                    if (subTreeWasMatched)
                    {
                        return;
                    }

                    SubTrees.Add(new Tree(current) { [remaining] = value });
                }
            }
        }
    }
}