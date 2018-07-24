using System;

namespace ConsoleApp1
{
    public static class Extensions
    {
        public static void Dump(this string arg) => Console.WriteLine(arg);
    }
}
