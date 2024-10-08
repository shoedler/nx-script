//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Projects/nx-script/nx-script-basic///src//Nx.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class NxLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		OR=1, AND=2, EQ=3, NEQ=4, GT=5, LT=6, GTEQ=7, LTEQ=8, PLUS=9, MINUS=10, 
		MULT=11, DIV=12, MOD=13, POW=14, NOT=15, COMMA=16, SCOL=17, ASSIGN=18, 
		OPAR=19, CPAR=20, OBRACE=21, CBRACE=22, TRUE=23, FALSE=24, NIL=25, IF=26, 
		ELSE=27, WHILE=28, LOG=29, ID=30, INT=31, FLOAT=32, STRING=33, COMMENT=34, 
		SPACE=35, OTHER=36;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"OR", "AND", "EQ", "NEQ", "GT", "LT", "GTEQ", "LTEQ", "PLUS", "MINUS", 
		"MULT", "DIV", "MOD", "POW", "NOT", "COMMA", "SCOL", "ASSIGN", "OPAR", 
		"CPAR", "OBRACE", "CBRACE", "TRUE", "FALSE", "NIL", "IF", "ELSE", "WHILE", 
		"LOG", "ID", "INT", "FLOAT", "STRING", "COMMENT", "SPACE", "OTHER"
	};


	public NxLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public NxLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'||'", "'&&'", "'=='", "'!='", "'>'", "'<'", "'>='", "'<='", "'+'", 
		"'-'", "'*'", "'/'", "'%'", "'^'", "'!'", "','", "';'", "'='", "'('", 
		"')'", "'{'", "'}'", "'true'", "'false'", "'nil'", "'if'", "'else'", "'while'", 
		"'log'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "OR", "AND", "EQ", "NEQ", "GT", "LT", "GTEQ", "LTEQ", "PLUS", "MINUS", 
		"MULT", "DIV", "MOD", "POW", "NOT", "COMMA", "SCOL", "ASSIGN", "OPAR", 
		"CPAR", "OBRACE", "CBRACE", "TRUE", "FALSE", "NIL", "IF", "ELSE", "WHILE", 
		"LOG", "ID", "INT", "FLOAT", "STRING", "COMMENT", "SPACE", "OTHER"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Nx.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static NxLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,36,216,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,1,0,1,0,1,0,1,1,1,1,1,1,1,2,1,2,1,2,1,3,1,3,1,3,1,4,1,4,1,5,1,5,1,
		6,1,6,1,6,1,7,1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,11,1,11,1,12,1,12,1,
		13,1,13,1,14,1,14,1,15,1,15,1,16,1,16,1,17,1,17,1,18,1,18,1,19,1,19,1,
		20,1,20,1,21,1,21,1,22,1,22,1,22,1,22,1,22,1,23,1,23,1,23,1,23,1,23,1,
		23,1,24,1,24,1,24,1,24,1,25,1,25,1,25,1,26,1,26,1,26,1,26,1,26,1,27,1,
		27,1,27,1,27,1,27,1,27,1,28,1,28,1,28,1,28,1,29,1,29,5,29,159,8,29,10,
		29,12,29,162,9,29,1,30,4,30,165,8,30,11,30,12,30,166,1,31,4,31,170,8,31,
		11,31,12,31,171,1,31,1,31,5,31,176,8,31,10,31,12,31,179,9,31,1,31,1,31,
		4,31,183,8,31,11,31,12,31,184,3,31,187,8,31,1,32,1,32,1,32,1,32,5,32,193,
		8,32,10,32,12,32,196,9,32,1,32,1,32,1,33,1,33,1,33,1,33,5,33,204,8,33,
		10,33,12,33,207,9,33,1,33,1,33,1,34,1,34,1,34,1,34,1,35,1,35,0,0,36,1,
		1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,12,25,13,27,14,29,
		15,31,16,33,17,35,18,37,19,39,20,41,21,43,22,45,23,47,24,49,25,51,26,53,
		27,55,28,57,29,59,30,61,31,63,32,65,33,67,34,69,35,71,36,1,0,6,3,0,65,
		90,95,95,97,122,4,0,48,57,65,90,95,95,97,122,1,0,48,57,3,0,10,10,13,13,
		34,34,2,0,10,10,13,13,3,0,9,10,13,13,32,32,224,0,1,1,0,0,0,0,3,1,0,0,0,
		0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,
		0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,
		27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,
		0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,
		0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,
		1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,
		0,0,71,1,0,0,0,1,73,1,0,0,0,3,76,1,0,0,0,5,79,1,0,0,0,7,82,1,0,0,0,9,85,
		1,0,0,0,11,87,1,0,0,0,13,89,1,0,0,0,15,92,1,0,0,0,17,95,1,0,0,0,19,97,
		1,0,0,0,21,99,1,0,0,0,23,101,1,0,0,0,25,103,1,0,0,0,27,105,1,0,0,0,29,
		107,1,0,0,0,31,109,1,0,0,0,33,111,1,0,0,0,35,113,1,0,0,0,37,115,1,0,0,
		0,39,117,1,0,0,0,41,119,1,0,0,0,43,121,1,0,0,0,45,123,1,0,0,0,47,128,1,
		0,0,0,49,134,1,0,0,0,51,138,1,0,0,0,53,141,1,0,0,0,55,146,1,0,0,0,57,152,
		1,0,0,0,59,156,1,0,0,0,61,164,1,0,0,0,63,186,1,0,0,0,65,188,1,0,0,0,67,
		199,1,0,0,0,69,210,1,0,0,0,71,214,1,0,0,0,73,74,5,124,0,0,74,75,5,124,
		0,0,75,2,1,0,0,0,76,77,5,38,0,0,77,78,5,38,0,0,78,4,1,0,0,0,79,80,5,61,
		0,0,80,81,5,61,0,0,81,6,1,0,0,0,82,83,5,33,0,0,83,84,5,61,0,0,84,8,1,0,
		0,0,85,86,5,62,0,0,86,10,1,0,0,0,87,88,5,60,0,0,88,12,1,0,0,0,89,90,5,
		62,0,0,90,91,5,61,0,0,91,14,1,0,0,0,92,93,5,60,0,0,93,94,5,61,0,0,94,16,
		1,0,0,0,95,96,5,43,0,0,96,18,1,0,0,0,97,98,5,45,0,0,98,20,1,0,0,0,99,100,
		5,42,0,0,100,22,1,0,0,0,101,102,5,47,0,0,102,24,1,0,0,0,103,104,5,37,0,
		0,104,26,1,0,0,0,105,106,5,94,0,0,106,28,1,0,0,0,107,108,5,33,0,0,108,
		30,1,0,0,0,109,110,5,44,0,0,110,32,1,0,0,0,111,112,5,59,0,0,112,34,1,0,
		0,0,113,114,5,61,0,0,114,36,1,0,0,0,115,116,5,40,0,0,116,38,1,0,0,0,117,
		118,5,41,0,0,118,40,1,0,0,0,119,120,5,123,0,0,120,42,1,0,0,0,121,122,5,
		125,0,0,122,44,1,0,0,0,123,124,5,116,0,0,124,125,5,114,0,0,125,126,5,117,
		0,0,126,127,5,101,0,0,127,46,1,0,0,0,128,129,5,102,0,0,129,130,5,97,0,
		0,130,131,5,108,0,0,131,132,5,115,0,0,132,133,5,101,0,0,133,48,1,0,0,0,
		134,135,5,110,0,0,135,136,5,105,0,0,136,137,5,108,0,0,137,50,1,0,0,0,138,
		139,5,105,0,0,139,140,5,102,0,0,140,52,1,0,0,0,141,142,5,101,0,0,142,143,
		5,108,0,0,143,144,5,115,0,0,144,145,5,101,0,0,145,54,1,0,0,0,146,147,5,
		119,0,0,147,148,5,104,0,0,148,149,5,105,0,0,149,150,5,108,0,0,150,151,
		5,101,0,0,151,56,1,0,0,0,152,153,5,108,0,0,153,154,5,111,0,0,154,155,5,
		103,0,0,155,58,1,0,0,0,156,160,7,0,0,0,157,159,7,1,0,0,158,157,1,0,0,0,
		159,162,1,0,0,0,160,158,1,0,0,0,160,161,1,0,0,0,161,60,1,0,0,0,162,160,
		1,0,0,0,163,165,7,2,0,0,164,163,1,0,0,0,165,166,1,0,0,0,166,164,1,0,0,
		0,166,167,1,0,0,0,167,62,1,0,0,0,168,170,7,2,0,0,169,168,1,0,0,0,170,171,
		1,0,0,0,171,169,1,0,0,0,171,172,1,0,0,0,172,173,1,0,0,0,173,177,5,46,0,
		0,174,176,7,2,0,0,175,174,1,0,0,0,176,179,1,0,0,0,177,175,1,0,0,0,177,
		178,1,0,0,0,178,187,1,0,0,0,179,177,1,0,0,0,180,182,5,46,0,0,181,183,7,
		2,0,0,182,181,1,0,0,0,183,184,1,0,0,0,184,182,1,0,0,0,184,185,1,0,0,0,
		185,187,1,0,0,0,186,169,1,0,0,0,186,180,1,0,0,0,187,64,1,0,0,0,188,194,
		5,34,0,0,189,193,8,3,0,0,190,191,5,34,0,0,191,193,5,34,0,0,192,189,1,0,
		0,0,192,190,1,0,0,0,193,196,1,0,0,0,194,192,1,0,0,0,194,195,1,0,0,0,195,
		197,1,0,0,0,196,194,1,0,0,0,197,198,5,34,0,0,198,66,1,0,0,0,199,200,5,
		47,0,0,200,201,5,47,0,0,201,205,1,0,0,0,202,204,8,4,0,0,203,202,1,0,0,
		0,204,207,1,0,0,0,205,203,1,0,0,0,205,206,1,0,0,0,206,208,1,0,0,0,207,
		205,1,0,0,0,208,209,6,33,0,0,209,68,1,0,0,0,210,211,7,5,0,0,211,212,1,
		0,0,0,212,213,6,34,0,0,213,70,1,0,0,0,214,215,9,0,0,0,215,72,1,0,0,0,10,
		0,160,166,171,177,184,186,192,194,205,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
