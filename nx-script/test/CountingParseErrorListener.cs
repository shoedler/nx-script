
using Antlr4.Runtime;

namespace NxScriptTest;

public class CountingParseErrorListener : IAntlrErrorListener<IToken>
{
    public readonly List<string> OccurredErrors = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        this.OccurredErrors.Add("line " + line + ":" + charPositionInLine + " " + msg);
    }
}

