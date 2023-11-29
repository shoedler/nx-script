using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

if (args.Length == 2)
{
    if (args[0] == "run")
    {
        RunFile(args[1]);
        Environment.Exit(0);
    }

    if (args[0] == "watch")
    {
        WatchFile(args[1]);
        Environment.Exit(1);
    }
}

UsageExit();

void RunFile(string path)
{
    var input = File.ReadAllText(path);

    var stream = CharStreams.fromString(input);
    var lexer = new NxLexer(stream);
    var tokens = new CommonTokenStream(lexer);
    var parser = new NxParser(tokens)
    {
        BuildParseTree = true
    };

    IParseTree tree = parser.parse();

    var visitor = new NxEvalVisitor();
    var value = visitor.Visit(tree);

    Console.WriteLine(value.AsString());
}

void WatchFile(string path)
{
    var watcher = new FileSystemWatcher();

    watcher.Path = Path.GetDirectoryName(path);
    watcher.Filter = Path.GetFileName(path);

    watcher.NotifyFilter = NotifyFilters.LastWrite;

    watcher.Changed += OnChanged;
    watcher.EnableRaisingEvents = true;

    Console.WriteLine("INFO: Watching file: " + path);
    Console.WriteLine("INFO: Press enter to exit.");
    Console.ReadLine();
}

void OnChanged(object source, FileSystemEventArgs e)
{
    Console.WriteLine("INFO: File changed: " + e.FullPath);

    try
    {
        RunFile(e.FullPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR: " + ex.Message);
    }
}

void UsageExit()
{
    Console.WriteLine("Usage: nxs <command?> <path>");
    Console.WriteLine("Commands:");
    Console.WriteLine("    run   - run a file");
    Console.WriteLine("    watch - watch and run a file");
    Environment.Exit(1);
}