using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;
using System.Diagnostics;

// TODO: Test DiagnosticParseErrorListener by Sam
// TODO: Add type inference examples with l/r associativity, e.g. true + 0 vs. 0 + true
// TODO: Add type specific intrinsic stuff, like "length" for strings, arrays, etc.
// TODO: Add string interpolation

Terminal.EnableVirtualTerminalOutput();

if (args.Length == 2)
{
    if (args[0] == "run")
    {
        InvalidPathExit(args[1]);
        Environment.Exit(RunFile(args[1]));
    }

    if (args[0] == "watch")
    {
        InvalidPathExit(args[1]);
        Environment.Exit(WatchFile(args[1]));
    }
}

UsageExit();

return;

int RunFile(string path)
{
    WaitForFileSync(path);

    var input = File.ReadAllText(path);

    var stream = CharStreams.fromString(input);
    var lexer = new NxLexer(stream);
    var tokens = new CommonTokenStream(lexer);
    var parser = new NxParser(tokens)
    {
        BuildParseTree = true
    };

    IParseTree tree = parser.parse();

    var visitor = new NxEvalVisitor(path);

    var sw = Stopwatch.StartNew();
    var value = visitor.Visit(tree);
    sw.Stop();

    Console.WriteLine(value.AsString());

    Terminal.Info($"Took {sw.ElapsedMilliseconds} ms");

    return 0;
}

int WatchFile(string path)
{
    // File-changed handler
    void OnChanged(object? source, FileSystemEventArgs e)
    {
        Console.Clear();

        Terminal.Info($"INFO: File {e.FullPath} changed, re-running...");
        Terminal.Separate();

        try
        {
            RunFile(e.FullPath);
        }
        catch (Exception ex)
        {
            Terminal.Error(ex.Message);
        }

        Terminal.Info($"Watching file {e.FullPath}");
        Terminal.Info("Press enter to exit.");
        Terminal.Separate();
    }

    WaitForFileSync(path);

    var watcher = new FileSystemWatcher
    {
        EnableRaisingEvents = true,
        Filter = Path.GetFileName(path),
        InternalBufferSize = 0,
        NotifyFilter = NotifyFilters.LastWrite,
        Path = Path.GetDirectoryName(path)!,
    };

    watcher.Changed += OnChanged;

    // Trigger initially
    OnChanged(null, new FileSystemEventArgs(WatcherChangeTypes.All, watcher.Path, watcher.Filter));

    Console.ReadLine();

    return 0;
}

void UsageExit()
{
    Terminal.Info("Usage: nxs <command?> <path>");
    Console.WriteLine("       Commands:");
    Console.WriteLine("         run   - run a file");
    Console.WriteLine("         watch - watch and run a file");
    Environment.Exit(1);
}

void InvalidPathExit(string path)
{
    if (File.Exists(path)) return;
    Terminal.Error($"File {path} does not exist");
    Environment.Exit(1);
}

void WaitForFileSync(string path)
{
    bool IsFileLocked(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            stream.Close();
        }
        catch (IOException)
        {
            return true;
        }

        return false;
    }

    while (IsFileLocked(path))
    {
        Thread.Sleep(400);
    }
}