//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Nx.g4 by ANTLR 4.13.1

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
		MULT=11, DIV=12, MOD=13, POW=14, NOT=15, LAMBDA=16, DOT=17, COMMA=18, 
		SCOL=19, COLON=20, ASSIGN=21, OPAR=22, CPAR=23, OBRACE=24, CBRACE=25, 
		OBRACK=26, CBRACK=27, TRUE=28, FALSE=29, NIL=30, IF=31, ELSE=32, WHILE=33, 
		LOG=34, FN=35, RETURN=36, ID=37, INT=38, FLOAT=39, STRING=40, COMMENT=41, 
		SPACE=42, OTHER=43;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"OR", "AND", "EQ", "NEQ", "GT", "LT", "GTEQ", "LTEQ", "PLUS", "MINUS", 
		"MULT", "DIV", "MOD", "POW", "NOT", "LAMBDA", "DOT", "COMMA", "SCOL", 
		"COLON", "ASSIGN", "OPAR", "CPAR", "OBRACE", "CBRACE", "OBRACK", "CBRACK", 
		"TRUE", "FALSE", "NIL", "IF", "ELSE", "WHILE", "LOG", "FN", "RETURN", 
		"ID", "INT", "FLOAT", "STRING", "COMMENT", "SPACE", "OTHER"
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
		"'-'", "'*'", "'/'", "'%'", "'^'", "'!'", "'->'", "'.'", "','", "';'", 
		"':'", "'='", "'('", "')'", "'{'", "'}'", "'['", "']'", "'true'", "'false'", 
		"'nil'", "'if'", "'else'", "'while'", "'log'", "'fn'", "'ret'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "OR", "AND", "EQ", "NEQ", "GT", "LT", "GTEQ", "LTEQ", "PLUS", "MINUS", 
		"MULT", "DIV", "MOD", "POW", "NOT", "LAMBDA", "DOT", "COMMA", "SCOL", 
		"COLON", "ASSIGN", "OPAR", "CPAR", "OBRACE", "CBRACE", "OBRACK", "CBRACK", 
		"TRUE", "FALSE", "NIL", "IF", "ELSE", "WHILE", "LOG", "FN", "RETURN", 
		"ID", "INT", "FLOAT", "STRING", "COMMENT", "SPACE", "OTHER"
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
		4,0,43,248,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,42,
		7,42,1,0,1,0,1,0,1,1,1,1,1,1,1,2,1,2,1,2,1,3,1,3,1,3,1,4,1,4,1,5,1,5,1,
		6,1,6,1,6,1,7,1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,11,1,11,1,12,1,12,1,
		13,1,13,1,14,1,14,1,15,1,15,1,15,1,16,1,16,1,17,1,17,1,18,1,18,1,19,1,
		19,1,20,1,20,1,21,1,21,1,22,1,22,1,23,1,23,1,24,1,24,1,25,1,25,1,26,1,
		26,1,27,1,27,1,27,1,27,1,27,1,28,1,28,1,28,1,28,1,28,1,28,1,29,1,29,1,
		29,1,29,1,30,1,30,1,30,1,31,1,31,1,31,1,31,1,31,1,32,1,32,1,32,1,32,1,
		32,1,32,1,33,1,33,1,33,1,33,1,34,1,34,1,34,1,35,1,35,1,35,1,35,1,36,1,
		36,5,36,191,8,36,10,36,12,36,194,9,36,1,37,4,37,197,8,37,11,37,12,37,198,
		1,38,4,38,202,8,38,11,38,12,38,203,1,38,1,38,5,38,208,8,38,10,38,12,38,
		211,9,38,1,38,1,38,4,38,215,8,38,11,38,12,38,216,3,38,219,8,38,1,39,1,
		39,1,39,1,39,5,39,225,8,39,10,39,12,39,228,9,39,1,39,1,39,1,40,1,40,1,
		40,1,40,5,40,236,8,40,10,40,12,40,239,9,40,1,40,1,40,1,41,1,41,1,41,1,
		41,1,42,1,42,0,0,43,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,
		23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,20,41,21,43,22,45,23,
		47,24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,63,32,65,33,67,34,69,35,
		71,36,73,37,75,38,77,39,79,40,81,41,83,42,85,43,1,0,6,3,0,65,90,95,95,
		97,122,4,0,48,57,65,90,95,95,97,122,1,0,48,57,3,0,10,10,13,13,34,34,2,
		0,10,10,13,13,3,0,9,10,13,13,32,32,256,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,
		0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,
		1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,
		0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,
		1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,
		0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,
		1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,
		0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,
		1,0,0,0,0,85,1,0,0,0,1,87,1,0,0,0,3,90,1,0,0,0,5,93,1,0,0,0,7,96,1,0,0,
		0,9,99,1,0,0,0,11,101,1,0,0,0,13,103,1,0,0,0,15,106,1,0,0,0,17,109,1,0,
		0,0,19,111,1,0,0,0,21,113,1,0,0,0,23,115,1,0,0,0,25,117,1,0,0,0,27,119,
		1,0,0,0,29,121,1,0,0,0,31,123,1,0,0,0,33,126,1,0,0,0,35,128,1,0,0,0,37,
		130,1,0,0,0,39,132,1,0,0,0,41,134,1,0,0,0,43,136,1,0,0,0,45,138,1,0,0,
		0,47,140,1,0,0,0,49,142,1,0,0,0,51,144,1,0,0,0,53,146,1,0,0,0,55,148,1,
		0,0,0,57,153,1,0,0,0,59,159,1,0,0,0,61,163,1,0,0,0,63,166,1,0,0,0,65,171,
		1,0,0,0,67,177,1,0,0,0,69,181,1,0,0,0,71,184,1,0,0,0,73,188,1,0,0,0,75,
		196,1,0,0,0,77,218,1,0,0,0,79,220,1,0,0,0,81,231,1,0,0,0,83,242,1,0,0,
		0,85,246,1,0,0,0,87,88,5,124,0,0,88,89,5,124,0,0,89,2,1,0,0,0,90,91,5,
		38,0,0,91,92,5,38,0,0,92,4,1,0,0,0,93,94,5,61,0,0,94,95,5,61,0,0,95,6,
		1,0,0,0,96,97,5,33,0,0,97,98,5,61,0,0,98,8,1,0,0,0,99,100,5,62,0,0,100,
		10,1,0,0,0,101,102,5,60,0,0,102,12,1,0,0,0,103,104,5,62,0,0,104,105,5,
		61,0,0,105,14,1,0,0,0,106,107,5,60,0,0,107,108,5,61,0,0,108,16,1,0,0,0,
		109,110,5,43,0,0,110,18,1,0,0,0,111,112,5,45,0,0,112,20,1,0,0,0,113,114,
		5,42,0,0,114,22,1,0,0,0,115,116,5,47,0,0,116,24,1,0,0,0,117,118,5,37,0,
		0,118,26,1,0,0,0,119,120,5,94,0,0,120,28,1,0,0,0,121,122,5,33,0,0,122,
		30,1,0,0,0,123,124,5,45,0,0,124,125,5,62,0,0,125,32,1,0,0,0,126,127,5,
		46,0,0,127,34,1,0,0,0,128,129,5,44,0,0,129,36,1,0,0,0,130,131,5,59,0,0,
		131,38,1,0,0,0,132,133,5,58,0,0,133,40,1,0,0,0,134,135,5,61,0,0,135,42,
		1,0,0,0,136,137,5,40,0,0,137,44,1,0,0,0,138,139,5,41,0,0,139,46,1,0,0,
		0,140,141,5,123,0,0,141,48,1,0,0,0,142,143,5,125,0,0,143,50,1,0,0,0,144,
		145,5,91,0,0,145,52,1,0,0,0,146,147,5,93,0,0,147,54,1,0,0,0,148,149,5,
		116,0,0,149,150,5,114,0,0,150,151,5,117,0,0,151,152,5,101,0,0,152,56,1,
		0,0,0,153,154,5,102,0,0,154,155,5,97,0,0,155,156,5,108,0,0,156,157,5,115,
		0,0,157,158,5,101,0,0,158,58,1,0,0,0,159,160,5,110,0,0,160,161,5,105,0,
		0,161,162,5,108,0,0,162,60,1,0,0,0,163,164,5,105,0,0,164,165,5,102,0,0,
		165,62,1,0,0,0,166,167,5,101,0,0,167,168,5,108,0,0,168,169,5,115,0,0,169,
		170,5,101,0,0,170,64,1,0,0,0,171,172,5,119,0,0,172,173,5,104,0,0,173,174,
		5,105,0,0,174,175,5,108,0,0,175,176,5,101,0,0,176,66,1,0,0,0,177,178,5,
		108,0,0,178,179,5,111,0,0,179,180,5,103,0,0,180,68,1,0,0,0,181,182,5,102,
		0,0,182,183,5,110,0,0,183,70,1,0,0,0,184,185,5,114,0,0,185,186,5,101,0,
		0,186,187,5,116,0,0,187,72,1,0,0,0,188,192,7,0,0,0,189,191,7,1,0,0,190,
		189,1,0,0,0,191,194,1,0,0,0,192,190,1,0,0,0,192,193,1,0,0,0,193,74,1,0,
		0,0,194,192,1,0,0,0,195,197,7,2,0,0,196,195,1,0,0,0,197,198,1,0,0,0,198,
		196,1,0,0,0,198,199,1,0,0,0,199,76,1,0,0,0,200,202,7,2,0,0,201,200,1,0,
		0,0,202,203,1,0,0,0,203,201,1,0,0,0,203,204,1,0,0,0,204,205,1,0,0,0,205,
		209,5,46,0,0,206,208,7,2,0,0,207,206,1,0,0,0,208,211,1,0,0,0,209,207,1,
		0,0,0,209,210,1,0,0,0,210,219,1,0,0,0,211,209,1,0,0,0,212,214,5,46,0,0,
		213,215,7,2,0,0,214,213,1,0,0,0,215,216,1,0,0,0,216,214,1,0,0,0,216,217,
		1,0,0,0,217,219,1,0,0,0,218,201,1,0,0,0,218,212,1,0,0,0,219,78,1,0,0,0,
		220,226,5,34,0,0,221,225,8,3,0,0,222,223,5,34,0,0,223,225,5,34,0,0,224,
		221,1,0,0,0,224,222,1,0,0,0,225,228,1,0,0,0,226,224,1,0,0,0,226,227,1,
		0,0,0,227,229,1,0,0,0,228,226,1,0,0,0,229,230,5,34,0,0,230,80,1,0,0,0,
		231,232,5,47,0,0,232,233,5,47,0,0,233,237,1,0,0,0,234,236,8,4,0,0,235,
		234,1,0,0,0,236,239,1,0,0,0,237,235,1,0,0,0,237,238,1,0,0,0,238,240,1,
		0,0,0,239,237,1,0,0,0,240,241,6,40,0,0,241,82,1,0,0,0,242,243,7,5,0,0,
		243,244,1,0,0,0,244,245,6,41,0,0,245,84,1,0,0,0,246,247,9,0,0,0,247,86,
		1,0,0,0,10,0,192,198,203,209,216,218,224,226,237,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
