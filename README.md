# Introduction

This project is a command line argument parser for .NET. Its purpose is to be incredibly easy to use.

Most libraries on the subject are just for parsing the arguments to your executable. Something like

```csharp
var options = SomeLibrary.Parse<MyOptions>();
```

The problem with this is twofold: first, you must then write the branching logic yourself to execute whatever logic you want based on what was set on your options object and second, 
this has no support for _verbs_ at the command line (more on that later).

My goal with Richiban.CommandLine was to create a library that felt--as much as possible--as if you could simple call your C# methods directly from the command line. For example, if
I have a method called `ProcessItems` that takes two arguments: an `int batchSize` and a `bool waitBetweenBatches` then I want to write absolutely as little code as I possibly can
apart from:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        // ... Some magic parsing that I don't care about!
    }

    public void ProcessItems(int batchSize, bool waitBetweenBatches = false)
    {
        // ... Implementation goes here
    }
}
```

I should then be able to call it like this:

```batchfile
myApp.exe 1000 /waitBetweenBatches
```

Well, I feel that this has been achieved with Richiban.CommandLine; this example would be implemented like this:

```csharp
using Richiban.CommandLine;

public class Program
{
    public static void Main(string[] args) => CommandLine.Execute(args);

    [CommandLine]
    public void ProcessItems(int batchSize, bool waitBetweenBatches = false)
    {
        // ... Implementation goes here
    }
}
```

which can, indeed, be called like this:

```batchfile
myApp.exe 1000 /waitBetweenBatches
```

As you can, once I've installed the Richiban.CommandLine NuGet package I've had to write almost no code to make the method callable from the command line.

Multiple methods can be tagged. All that matters is that the command line arguments have unique names‡.

```csharp
public class Program
{
    public static void Main(string[] args) => CommandLine.Execute(args);

    [CommandLine]
    public void ProcessItems(int batchSize, bool waitBetweenBatches = false)
    {
        // ... Implementation goes here
    }

    [CommandLine]
    public void WriteToLogFile(string contents)
    {
        // ... Implementation goes here
    }
}
```

> ‡For more complicated scenarios (i.e. when you have a lot of methods) see the section on _verbs_ below.

## What are some of the features of Richiban.CommandLine?

### It supports Unix-style, Windows-style and Powershell-style argument passing

Unix-style looks like this: 

```sh
someApp --paramA=valueA --flag1
```

Windows-style looks like this: 

```batchfile
someApp.exe /paramA:valueA /flag1
```

Powershell-style looks like this: 

```powershell
someApp -paramA valueA -flag1
```

These are all supported out of the box†, and can even be mixed and matched (although this is not recommended). For example, this is perfectly acceptable:

```
someApp.exe /paramA:valueA --flag1 -paramB:valueB
```

> †Note: On Unix systems (or, more accurately, systems where `/` is the path separator) the Windows-style parameter name parsing is disabled; this is due to the ambiguity with path
> arguments

### The order of supplied arguments doesn't matter (if names are given)

```sh
myApp --paramA=valueA --paramB=valueB
```
and
```sh
myApp --paramB=valueB --paramA=valueA
```
are considered equal, because the names have been given; i.e. the order is not important because it's unambiguous. Note that if you don't give the names:
```sh
myApp valueB valueA
```
then it still works, but the order is now important.

### Verbs are supported

We're getting into more advanced terratory now. Let's look at an example from the command line reference for Git:

```sh
git remote add origin http://example.com/project.git
```

In the example above, the tokens `remote` and `add` are _verbs_, and `origin` and `http://example.com/project.git` are the arguments. Richiban.CommandLine supports verbs! Since, technically, only the last of these tokens is really a verb we call them _routes_. This example with two route parts and two arguments would look like this:

```csharp
[CommandLine, Route("remote", "add")]
public void AddRemote(string remoteName, Uri remoteUri)
{
    // ... Implementation goes here
}
```

> Note that here we have an argument of type `Uri`. See [Argument types are converted].

For the purposes of code organisation and not repeating oneself, you can also put route attributes
on the containing class. So, instead of having to write:

```csharp
class MyRemoteActions
{
	[CommandLine, Route("remote", "add")]
	public void AddRemote(...) 
	{ ... }

	[CommandLine, Route("remote", "remove")]
	public void RemoveRemote(...)
	{ ... }

	[CommandLine, Route("remote", "rename")]
	public void RemoveRemote(...)
	{ ... }
}
```

you can instead write:

```csharp
[Route("remote")]
class MyRemoteActions
{
	[CommandLine, Route("add")]
	public void AddRemote(...)
	{ ... }

	[CommandLine, Route("remove")]
	public void RemoveRemote(...)
	{ ... }

	[CommandLine, Route("rename")]
	public void RenameRemote(...) =>
	{ ... }
}
```

Note also that you can infer the route name by using the route attribute bare, like so:

```csharp
[CommandLine, Route]
public void MyAction()
{
	// This method will respond to the "MyAction" route (case-insensitive)
}
```

### Argument types are converted 

Note that arguments do not have to be strings.

The rule is that arguments must either:
 * Be of type `string`
 * implement `IConvertible` (the conversion is done by `Convert.ChangeType(...)`)
 * have a constructor that takes a `string` or `string[]` as argument
 * be of an Enum type (then the conversion is done by `Enum.Parse(...)`)

Some example types that work out of the box:
* string
* bool
* int
* Uri
* FileInfo
and many more

### Compatible with dependency injection frameworks

It would be pretty ugly if your methods were nicely reachable from the command line, but they all looked like this:

```csharp
public class SomeClass
{
    [CommandLine, Route("method1")]
    public void Method1(string argument)
    {
        var serviceA = ObjectContainer.Instance.Resolve<ServiceA>();
        var serviceB = ObjectContainer.Instance.Resolve<ServiceB>();
        var serviceC = ObjectContainer.Instance.Resolve<ServiceC>();

        // ... Implementation goes here
    }
}
```

Wouldn't it be much better if proper dependency injection like this was possible instead?

```csharp
public class SomeClass
{
    private readonly ServiceA _serviceA;
    private readonly ServiceB _serviceB;
    private readonly ServiceC _serviceC;

    public SomeClass(ServiceA serviceA, ServiceB serviceB, ServiceC serviceC) =>
        (_serviceA, _serviceB, _serviceC) = (serviceA, serviceB, serviceC);

    [CommandLine, Route("method1")]
    public void Method1(string argument)
    {
        // ... Implementation goes here
    }
}
```

Well, it is easy with Richiban.CommandLine! First, we note that there is an overload of `CommandLine.Execute` that takes a `CommandLineConfiguration` object. Let's look at the
`CommandLineConfiguration` type:

```csharp
    public class CommandLineConfiguration
    {
        public Action<string> HelpOutput { get; set; }

        public Func<Type, object> ObjectFactory { get; set; }

        public Assembly AssemblyToScan { get; set; }

        public static CommandLineConfiguration GetDefault() => ...
    }
```

Of interest is the `ObjectFactory` property. It's a `Func<Type, object>`, which is the most generic possible definition of a factory (it's a function that takes a `Type` as argument and 
returns an `object`). The `ObjectFactory` property has a setter, so we can provide whatever implementation we want. Since it's a `Func` we don't even have to implement an interface, we
can simply configure like this:

```csharp
public static void Main(string[] args)
{
    // Instantiate our favourite DI container. CastleWindsor is only an example; it could be anything.
    var container = new WindsorContainer();

    var config = CommandLineConfiguration.GetDefault();
    config.ObjectFactory = container.Resolve;

    CommandLine.Execute(config, args);
}
```

### Automatic help

One of Richiban.CommandLine's best features is its ability to auto generate help for the users of your command line app.

For example, let's assume we have the following application:

```csharp
[CommandLine, Route("method1")]
public void Method1(string someArgument, bool someFlag = false)
{
    //...
}
```

But, when it comes time to use this app, our user can't quite remember the order of arguments or their exact names. Using the auto-help feature, they can enter:

```bash
> myapp method1 -?
```

or

```
> myapp help method1
```

and they will see the following:

```
Help for method1:
    myapp method1 <someArgument> [--someFlag]
```

> Note the `--` prefix to `someFlag`. Since there isn't a 'correct' way of writing flags or named arguments in Richiban.CommandLine, a best guess is made when writing auto-help. Currently this is relies on the path separator for your system, so on Windows this would appear as: `myapp method1 <someArgument> [/someFlag]`.

If the user calls `help` and the arguments they have supplied are ambiguous then auto-help will be displayed for all routes that even partially match what they did supply.

#### XML comments from your methods and classes will appear in auto-help

Auto help can even pick out your XML comments! (Don't forget to enable the export of your XML comments in your project file).

Let's say we have the following method:

```csharp
/// <summary>
/// This is a comment for my method.
/// </summary>
/// <param name="param1">The first parameter</param>
/// <param name="param2">The second parameter</param>
/// <param name="someFlag">A flag that does something interesting</param>
[CommandLine, Route("method")]
public void MyMethod(string param1, FileInfo param2, [ShortForm('f')] bool someFlag = false)
{
	//...
}
```

This method will then appear in help as follows:

```
Richiban.CommandLine.Samples.exe method <param1> <param2> [-f|--someFlag]
    This is a comment for my method.

    Parameters:
        <param1> The first parameter

        <param2> The second parameter. Type: System.IO.FileInfo

        [-f|--someFlag] A flag that does something interesting
```

Note that because the type for parameter `param2` is 'interesting' (i.e. it isn't a `string` or `bool`) 
auto help has automatically noted the type here in the comments.

-------
That's about it for the readme. Please feel free to read the issues in this project to see what's coming further down the road or, if you dream up more features for Richiban.CommandLine, post an issue of your own. I also welcome (expected) PRs so contact me before starting any work.
