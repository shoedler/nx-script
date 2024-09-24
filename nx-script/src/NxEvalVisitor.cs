using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace NxScript;

public class NxEvalVisitor : AbstractParseTreeVisitor<NxValue>, INxVisitor<NxValue>
{
    public readonly Dictionary<string, NxValue> Variables = [];

    protected NxEvalVisitor? Upper = null;
    protected NxEvalVisitor? Fn = null;
    protected NxValue? ReturnValue = null;
    protected override NxValue DefaultResult => new NxValueNil(); // Always a new one!

    public NxEvalVisitor(string path)
    {
        // Preload variables
        this.Variables.Add("pi", new NxValueNumber((float)Math.PI));
        this.Variables.Add("cwd", new NxValueString(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(path)!)));

        // Preload "print" function - print to console
        this.Variables.Add("print", new NxValueFn((args) =>
        {
            args.ForEach(arg =>
            {
                var log = arg.Type switch
                {
                    NxValueType.String => $"{arg.AsString()}".InGreen(),
                    NxValueType.Bool => arg.AsString().InMagenta(),
                    NxValueType.Number => arg.AsString().InYellow(),
                    NxValueType.Seq or 
                    NxValueType.Obj or 
                    NxValueType.Fn => arg.AsString().InCyan(),
                    NxValueType.Nil => "nil".InBlue(),
                    _ => arg.AsString()
                };

                Console.Write(log + " ");
            });

            Console.WriteLine();
            return this.DefaultResult;
        }));

        // Preload "read" function - read files from disk
        this.Variables.Add("read", new NxValueFn(args =>
        {
            var content = File.ReadAllText(args[0].AsString());
            return new NxValueString(content);
        }));

        // Preload "write" function - write to disk
        this.Variables.Add("write", new NxValueFn(args =>
        {
            File.WriteAllText(args[0].AsString(), args[1].AsString() ?? "");
            return new NxValueString(args[0].AsString());
        }));

        // Preload "help" function - show vars & funcs
        this.Variables.Add("help", new NxValueFn(args =>
        {
            Console.WriteLine("Variables in current scope:");
            this.Variables.AsEnumerable().ToList()
                .ForEach((pair) => Console.WriteLine($"{pair.Key}:\t{pair.Value.AsString()}"));
            return this.DefaultResult;
        }));

        // Preload "typeof" function - show type of value
        this.Variables.Add("typeof", new NxValueFn(args => new NxValueString(Enum.GetName(args[0].Type) ?? "<Unknown>")));

        // Preload "len" function - show length of array
        this.Variables.Add("len", new NxValueFn(args =>
        {
            var arg = args[0] ?? new NxValueNil();
            return new NxValueNumber(arg.AsSeq().Count);
        }));

        // Preload "split" function - split string into array (default is \r\n
        this.Variables.Add("split_by_newline", new NxValueFn(args =>
        {
            var arg = args[0] ?? this.DefaultResult;
            var strings = arg.AsString().Split(Environment.NewLine).Select(str => new NxValueString(str) as NxValue).ToList();
            return new NxValueSeq(strings);
        }));
    }

    protected NxEvalVisitor(NxEvalVisitor upper)
    {
        // Preload "help" function - show vars & funcs
        this.Variables["help"] = new NxValueFn(args =>
        {
            Console.WriteLine("Variables in scope:");

            var parent = this;
            var scopes = new List<NxEvalVisitor> { this };

            while (parent.Upper is not null)
            {
                parent = parent.Upper;
                scopes.Add(parent);
            }

            scopes.Reverse();

            for (var i = 0; i < scopes.Count; i++)
            {
                var scope = scopes[i];
                var indent = new string(' ', i);
                Console.WriteLine($"{indent}[Scope {i}]:");
                scope.Variables.AsEnumerable().ToList()
                    .ForEach((pair) => Console.WriteLine($"{indent}{pair.Key}:\t{pair.Value.AsString()}"));
            }
                
            return new NxValueNil();
        });

        this.Upper = upper;
        this.Fn = upper.Fn;
    }

    public NxValue VisitParse([NotNull] NxParser.ParseContext context)
    {
        return this.VisitChildren(context);
    }

    public NxValue VisitBlock([NotNull] NxParser.BlockContext context)
    {
        var visitor = this.NewScope();
        return visitor.VisitChildren(context);
    }

    public NxValue VisitStat_block([NotNull] NxParser.Stat_blockContext context)
    {
        return this.VisitChildren(context);
    }

    ///
    /// Statements
    ///
    public NxValue VisitStat([NotNull] NxParser.StatContext context)
    {

       return this.VisitChildren(context);;
    }

    public NxValue VisitVar_declaration([NotNull] NxParser.Var_declarationContext context)
    {
        var value = this.Visit(context.expr());
        this.DeclareVariable(context.ID().GetText(), value, context);
        return value;
    }

    public NxValue VisitIf_stat([NotNull] NxParser.If_statContext context)
    {
        var expressions = context.expr();
        var statBlocks = context.stat_block();

        foreach (var (condition, stat) in expressions.Zip(statBlocks))
        {
            if (!this.Visit(condition).AsBool())
            {
                continue;
            }

            return this.Visit(stat);
        }

        if (statBlocks.Length > expressions.Length)
        {
            return this.Visit(statBlocks.Last());
        }

        return this.DefaultResult;
    }

    public NxValue VisitWhile_stat([NotNull] NxParser.While_statContext context)
    {
        var panic = 0;
        var lastValue = this.DefaultResult;

        while (this.Visit(context.expr()).AsBool())
        {
            lastValue = this.Visit(context.stat_block());
            if (panic++ <= Constants.Limits.MaxLoopIter) 
            {
                throw NxEvalException.FromContext($"Panic! Loop did not finish after {panic} iterations", context);
            }
        }

        return lastValue;
    }

    public NxValue VisitReturn([NotNull] NxParser.ReturnContext context)
    {
        var ret = this.VisitChildren(context);

        if (this.Fn is not null)
        {
            this.Fn.ReturnValue = ret;
            return this.Fn.ReturnValue;
        }

        return ret;
    }

    ///
    /// Literals
    ///
    public NxValue VisitArray_literal([NotNull] NxParser.Array_literalContext context)
    {
        var items = context.expr().Select(this.Visit).ToList();

        return new NxValueSeq(items);
    }

    public NxValue VisitObj_literal([NotNull] NxParser.Obj_literalContext context)
    {
        var atoms = context.atom().Select(this.Visit);
        var expressions = context.expr().Select(this.Visit);

        return new NxValueObj(atoms.Zip(expressions));
    }

    public NxValue VisitFn_literal([NotNull] NxParser.Fn_literalContext context)
    {
        var argNames = context.ID().Select(n => n.GetText());

        // Some manual optimization to ensure we don't create a new scope twice when running trough 
        // VisitBlock() (Affects fns with block bodies)
        var blockBody = context.stat_block().block();
        var statBody = context.stat_block().stat();

        IParseTree fnBody = blockBody is not null ? blockBody : statBody;

        // Closure
        var fn = new NxValueFn((args) =>
        {
            var fnVisitor = this.NewScope();

            foreach (var (argName, value) in argNames.Zip(args))
            {
                // We deliberaltely set it manually here. Bc if there's a an identically named variable above, it's not gonna be fun.
                fnVisitor.Variables[argName] = value ?? throw NxEvalException.FromContext($"Missing arg {argName}", context);
            }

            fnVisitor.Fn = fnVisitor;
            var ret = fnVisitor.Visit(fnBody);

            return fnVisitor.Fn.ReturnValue ?? ret ?? this.DefaultResult;
        });

        return fn;
    }

    ///
    /// Expressions
    /// 
    public NxValue VisitFnCallExpr([NotNull] NxParser.FnCallExprContext context)
    {
        var key = context.ID().GetText();
        var function = this.GetVariable(key, context);
        var args = context.expr().Select(this.Visit).ToList();

        return function.AsFn().Invoke(args);
    }

    public NxValue VisitIndexExpr([NotNull] NxParser.IndexExprContext context)
    {
        var array = this.Visit(context.expr().First());
        var index = this.Visit(context.expr().Last());

        return NxValue.Index(array, index);
    }

    public NxValue VisitMemberExpr([NotNull] NxParser.MemberExprContext context)
    {
        var obj = this.Visit(context.expr());
        var member = new NxValueString(context.ID().GetText());

        return NxValue.Member(obj, member);
    }

    public NxValue VisitArrayExpr([NotNull] NxParser.ArrayExprContext context)
    {
        return this.Visit(context.array_literal()); // Just pluck the literal, could also just return the default base.VisitChildren() but this is quicker
    }

    public NxValue VisitObjExpr([NotNull] NxParser.ObjExprContext context)
    {
        return this.Visit(context.obj_literal()); // Just pluck the literal, could also just return the default VisitChildren() but this is quicker
    }

    public NxValue VisitFnExpr([NotNull] NxParser.FnExprContext context)
    {
        return this.Visit(context.fn_literal()); // Just pluck the literal, could also just return the default VisitChildren() but this is quicker
    }

    public NxValue VisitPowExpr([NotNull] NxParser.PowExprContext context)
    {
        var right = this.Visit(context.expr()[1]);
        var left = this.Visit(context.expr()[0]);

        return new NxValueNumber((float)Math.Pow(left.AsNumber(), right.AsNumber()));
    }

    public NxValue VisitUnaryMinusExpr([NotNull] NxParser.UnaryMinusExprContext context)
    {
        var right = this.Visit(context.expr());

        return new NxValueNumber(-right.AsNumber());
    }

    public NxValue VisitNotExpr([NotNull] NxParser.NotExprContext context)
    {
        var right = this.Visit(context.expr());

        return new NxValueBool(!right.AsBool());
    }

    public NxValue VisitMultiplicationExpr([NotNull] NxParser.MultiplicationExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.MULT => NxValue.Multiply(left, right),
            NxParser.DIV => new NxValueNumber(left.AsNumber() / right.AsNumber()),
            NxParser.MOD => new NxValueNumber(left.AsNumber() % right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this multiplicative operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAdditiveExpr([NotNull] NxParser.AdditiveExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.PLUS => NxValue.Add(left, right),
            NxParser.MINUS => new NxValueNumber(left.AsNumber() - right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this additive operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitRelationalExpr([NotNull] NxParser.RelationalExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.LTEQ => new NxValueBool(left.AsNumber() <= right.AsNumber()),
            NxParser.GTEQ => new NxValueBool(left.AsNumber() >= right.AsNumber()),
            NxParser.LT => new NxValueBool(left.AsNumber() < right.AsNumber()),
            NxParser.GT => new NxValueBool(left.AsNumber() > right.AsNumber()),
            _ => throw NxEvalException.FromContext($"Don't know this relational operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitEqualityExpr([NotNull] NxParser.EqualityExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return context.op.Type switch
        {
            NxParser.EQ => NxValue.Eq(left,right),
            NxParser.NEQ => NxValue.Neq(left, right),
            _ => throw NxEvalException.FromContext($"Don't know this equality operator '{context.GetText()}'", context)
        };
    }

    public NxValue VisitAndExpr([NotNull] NxParser.AndExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return new NxValueBool(left.AsBool() && right.AsBool());
    }

    public NxValue VisitOrExpr([NotNull] NxParser.OrExprContext context)
    {
        var left = this.Visit(context.expr()[0]);
        var right = this.Visit(context.expr()[1]);

        return new NxValueBool(left.AsBool() || right.AsBool());
    }

    public NxValue VisitAssignExpr([NotNull] NxParser.AssignExprContext context)
    {
        var right = this.Visit(context.expr()[1]);
        var left = this.Visit(context.expr()[0]);

        // TODO: Decide if we want RTL From LType
        // We could also create a new Variable (Results in a new reference which doesn't really work)
        // Could also make RTL from RType, which is MUCH more understandable. You kinda have to know the type of lvalue to 
        // understand what your assignment will do -> it converts rvalue to lvalue!
        NxValue.AssignInternalRTLFromLType(left, right);

        return left;
    }

    public NxValue VisitAtomExpr([NotNull] NxParser.AtomExprContext context)
    {
        return this.VisitChildren(context);
    }

    ///
    /// Atoms
    /// 
    public NxValue VisitParExpr([NotNull] NxParser.ParExprContext context) { return this.Visit(context.expr()); }
    public NxValue VisitNumberAtom([NotNull] NxParser.NumberAtomContext context) { return new NxValueNumber(context); }
    public NxValue VisitStringAtom([NotNull] NxParser.StringAtomContext context) { return new NxValueString(context); }
    public NxValue VisitBoolAtom([NotNull] NxParser.BoolAtomContext context) { return new NxValueBool(context); }
    public NxValue VisitNilAtom([NotNull] NxParser.NilAtomContext context) { return this.DefaultResult; }
    public NxValue VisitIdAtom([NotNull] NxParser.IdAtomContext context) { return this.GetVariable(context.ID().GetText(), context); }

    ///
    /// Visitor logic overrides
    /// 
    public override NxValue VisitChildren(IRuleNode node)
    {
        // Don't burn your fingers - this is extremely hot!
        var val = this.DefaultResult;
        var childCount = node.ChildCount;

        for (var i = 0; i < childCount; i++)
        {
            var child = node.GetChild(i);

            if (this.Fn?.ReturnValue != null)
            {
                // Use return value if there is one!
                return this.Fn.ReturnValue;
            }

            val = this.Visit(child);
        }

        return val;
    }

    ///
    /// Helpers
    /// 
    internal NxValue GetVariable(string key, ParserRuleContext context)
    {
        if (this.Variables.TryGetValue(key, out var value))
        {
            return value;
        }

        if (this.Upper is null)
        {
            throw NxEvalException.FromContext($"Variable '{key}' is not defined", context);
        }

        return this.Upper.GetVariable(key, context);
    }

    internal void DeclareVariable(string key, NxValue value, ParserRuleContext context)
    {
        if (this.Variables.ContainsKey(key))
        {
            throw NxEvalException.FromContext($"Variable '{key}' is already defined", context);
        }

        this.Variables[key] = value;
    }

    protected virtual NxEvalVisitor NewScope()
    {
        return new NxEvalVisitor(this);
    }
}