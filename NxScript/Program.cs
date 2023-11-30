using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

// TODO: Test DiagnosticParseErrorListener by Sam
// TODO: Add type inference examples with l/r associativity, e.g. true + 0 vs. 0 + true

#if DEBUG
args = new[] { "watch", "..\\..\\..\\SampleScript.nx" };
#endif

Terminal.EnableVirtualTerminalOutput();

if (args.Length == 2)
{
    if (args[0] == "run")
    {
        Environment.Exit(RunFile(args[1]));
    }

    if (args[0] == "watch")
    {
        Environment.Exit(WatchFile(args[1]));
    }
}

UsageExit();

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
    var value = visitor.Visit(tree);

    Console.WriteLine(value.AsString());

    return 0;
}

int WatchFile(string path)
{
    var watcher = new FileSystemWatcher();

    if (!File.Exists(path))
    {
        Terminal.Error($"File {path} does not exist");
        return 1;
    }

    watcher.Path = Path.GetDirectoryName(path)!;
    watcher.Filter = Path.GetFileName(path);

    watcher.NotifyFilter = NotifyFilters.LastWrite;

    watcher.Changed += OnChanged;
    watcher.EnableRaisingEvents = true;

    // Trigger initially
    OnChanged(null, new(WatcherChangeTypes.All, watcher.Path, watcher.Filter));

    Console.ReadLine();

    return 0;
}

void OnChanged(object source, FileSystemEventArgs e)
{
    Terminal.Info($"INFO: File {e.FullPath} changed");

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

void UsageExit()
{
    Terminal.Info("Usage: nxs <command?> <path>");
    Console.WriteLine("       Commands:");
    Console.WriteLine("         run   - run a file");
    Console.WriteLine("         watch - watch and run a file");
    Environment.Exit(1);
}

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

void WaitForFileSync(string path)
{
    while (IsFileLocked(path))
    {
        Thread.Sleep(400);
    }
}
