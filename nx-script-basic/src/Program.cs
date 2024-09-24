using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

static void Abort(string type, string message)
{
    Console.Error.WriteLine($"{type}: {message}");
    Environment.Exit(1);
}

if (args.Length != 1)
    Abort("IO ERROR", "Missing path to source file.");
if (!File.Exists(args[0]))
    Abort("IO ERROR", $"File {args[0]} does not exist.");

var input = File.ReadAllText(args[0]);
var stream = CharStreams.fromString(input);
var lexer = new NxLexer(stream);
var tokens = new CommonTokenStream(lexer);
var parser = new NxParser(tokens)
{
    BuildParseTree = true
};

// Parse source
IParseTree? tree = default;
try
{
    tree = parser.parse();
}
catch (Exception e)
{
    Abort("PARSE ERROR", e.Message + Environment.NewLine + e.StackTrace);
}

var visitor = new NxEvalVisitor();

// Evaluate tree
NxValue value = new();
try
{
    value = visitor.Visit(tree);
}
catch (Exception e)
{
    Abort("RUNTIME ERROR", e.Message + Environment.NewLine + e.StackTrace);
}

Console.WriteLine(value.AsString());