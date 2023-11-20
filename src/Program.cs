using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using NxScript;

if (args.Length != 1)
{
    Console.Error.WriteLine("Missing path to source file");
    Environment.ExitCode = 1;
    return;
}

var input = File.ReadAllText(args[0]);

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
