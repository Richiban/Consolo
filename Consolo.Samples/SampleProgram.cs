using System;

namespace Consolo.Samples
{
    /// <summary>
    /// A sample program that demonstrates how to use Consolo
    /// </summary>
    [Consolo("")]
    class TestCommands
    {
        /// <summary>
        /// A test command that takes a file as a parameter
        /// </summary>
        /// <param name="lines">The number of lines to display</param>
        /// <param name="prettyPrintMode">The display mode</param>
        [Consolo("log")]
        public static void WriteLog(
            int lines,
            [Consolo("pretty")] PrettyPrintMode prettyPrintMode = PrettyPrintMode.Normal)
        {
            Console.WriteLine($"Displaying {lines} lines (pretty = {prettyPrintMode})");
        }

        public enum PrettyPrintMode
        {
            /// <summary>
            /// Show log messages in their entirety
            /// </summary>
            Normal,

            /// <summary>
            /// Show the first line of each log message
            /// </summary>
            OneLine
        }

        /// <summary>
        /// A sample test command with a really long description that spans multiple lines
        /// and should cause a newline to be inserted in the help text so that we can see
        /// how it is formatted in the auto-generated help.
        /// </summary>
        /// <param name="argA">The first test parameter</param>
        /// <param name="argB">The second test parameter</param>
        /// <param name="testEnum">A test value using an Enum</param>
        [Consolo("test")]
        public static void TestMethod(
            [Consolo("a")] bool argA = false,
            [Consolo("b")] bool argB = false,
            TestEnum testEnum = TestEnum.A
        )
        {
            Console.WriteLine(new { argA, argB, testEnum });
        }

        public enum TestEnum
        {
            /// <summary>
            /// Test value A
            /// </summary>
            A,
            B,
            C,
            D
        }
    }

    [Consolo("greet")]
    class GreetCommands
    {
        /// <summary>
        /// Greets a person
        /// </summary>
        /// <param name="name">The name of the person to greet</param>
        /// <param name="loud">Whether or not to say it loud!</param>
        [Consolo("")]
        public static void Greet(
            string name,
            [Consolo(Alias = "l")] bool loud = false)
        {
            var message = $"Hello, {name}";

            if (loud)
            {
                message = message.ToUpper() + "!";
            }

            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Commands for managing remotes
    /// </summary>
    [Consolo("remote")]
    class RemoteCommands
    {
        /// <summary>
        /// List remotes
        /// </summary>
        [Consolo("")]
        public static void ListRemotes()
        {
            Console.WriteLine("Listing remotes");
        }

        /// <summary>
        /// Add a remote
        /// </summary>
        /// <param name="name">The name of the remote</param>
        /// <param name="url">The remote URL</param>
        /// <param name="allowClobber">Whether to overwrite an existing remote</param>
        [Consolo("add")]
        public static void AddRemote(
            string name,
            Uri url,
            [Consolo("force", Alias = "f")] bool allowClobber = false)
        {
            Console.WriteLine($"Adding remote {name}: {url} (allowClobber = {allowClobber})");
        }

        /// <summary>
        /// Remove a remote
        /// </summary>
        /// <param name="remoteName">The name of the remote to remove</param>
        [Consolo("remove")]
        public static void RemoveRemote(string remoteName)
        {
            Console.WriteLine($"Removing remote {remoteName}");
        }
    }

    [Consolo("clean")]
    class CleanCommands
    {
        /// <summary>
        /// Clean the working directory
        /// </summary>
        /// <param name="bypassGitIgnore">Bypass gitignore</param>
        /// <param name="includeDirectories">Include directories</param>
        /// <param name="force">Whether to force the clean</param>
        [Consolo("")]
        public static void Clean(
            [Consolo(Alias = "x")] bool bypassGitIgnore = false,
            [Consolo(Alias = "d")] bool includeDirectories = false,
            [Consolo(Alias = "f")] bool force = false)
        {
            Console.WriteLine($"Cleaning the working directory ({new { bypassGitIgnore, includeDirectories, force }})");
        }
    }
}