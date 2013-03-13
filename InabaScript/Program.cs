using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript
{
    public interface IElement
    {
        string Name { get; }
    }

    public interface IType : IElement
    {
        bool Accepts(IType rtype);
        IType DeTuple();
    }


    
    public interface IDeclaration : IElement
    {
    }

    public interface IExpression
    {
        IType Type { get; }
        IExpression Bind(Scope scope);
    }

    public interface IVariable
    {
        string Name { get; }
        IType Type { get; }
    }

    public interface IStatement
    {
        IStatement Bind(ref Scope scope);
    }
    

    class IntegerType : IType
    {
        public readonly long max;
        public readonly long min;

        public IntegerType(long num)
        {
            max = num;
            min = num;
        }

        public IntegerType(long min, long max)
        {
            this.max = max;
            this.min = min;
        }

        public bool IsWithin(IntegerRange other)
        {
            return (min >= other.min && max <= other.max);
        }

        public bool Accepts(IType rtype)
        {
            if (rtype.DeTuple() is IntegerType)
            {
                var inttype = (IntegerType)rtype.DeTuple();
                if (inttype.min >= min && inttype.max <= max)
                {
                    return true;
                }
            }
            return false;
        }

        public string Name
        {
            get
            {
                return min + ".." + max;
            }
        }

        public override string ToString()
        {
            return "integer:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    interface ISetMember
    {
        string Name { get; }
    }

    class ArrayType : IType
    {
        public readonly IType innerType;
        public readonly IntegerType sizeType;

        public ArrayType(IType innerType, IntegerType sizeType)
        {
            this.innerType = innerType;
            this.sizeType = sizeType;
        }

        public bool Accepts(IType rtype)
        {
            if (rtype is ArrayType)
            {
                ArrayType arrType = rtype as ArrayType;
                // special case
                if (arrType.sizeType.min == 0 && arrType.sizeType.max == 0 && arrType.innerType is UnknownType)
                {
                    return true;
                }

                return innerType.Accepts(arrType.innerType) && sizeType.Accepts(arrType.sizeType);
            }
            return false;
        }

        public string Name
        {
            get
            {
                return "[" + innerType.Name + "<" + sizeType.Name + ">" + "]";
            }
        }

        public override string ToString()
        {
            return "array:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    class UnknownType : IType
    {
        public bool Accepts(IType exp)
        {
            return true;
        }

        public string Name
        {
            get { return "unknown"; }
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    public class FunctionType : IType
    {
        public readonly IType ArgumentType;
        public readonly IType ReturnType;

        public FunctionType(IType argumentType, IType returnType)
        {
            this.ArgumentType = argumentType;
            this.ReturnType = returnType;
        }

        public bool Accepts(IType rtype)
        {
            return rtype.Name == Name;
        }

        public string Name
        {
            get
            {
                return ArgumentType.Name + "->" + ReturnType.Name;
            }
        }

        public override string ToString()
        {
            return "function:" + Name;
        }

        public IType DeTuple()
        {
            return this;
        }
    }


    class FunctionCall : IExpression, IStatement
    {

        public readonly IExpression Rhs;
        public readonly IExpression Lhs;

        public FunctionCall(IExpression lhs, IExpression rhs)
        {
            //if (!(lhs.Type is FunctionType))
            //{
            //    throw new Exception(lhs + " not a function!");
            //}

            //FunctionType lhsType = (FunctionType)lhs.Type;
            //if (!lhsType.ArgumentType.Accepts(rhs.Type))
            //{
            //    throw new Exception(lhs.Type + " does not accept " + rhs.Type);
            //}

            //Type = lhsType.ReturnType;

            Lhs = lhs;
            Rhs = rhs;
        }

        public FunctionCall(IExpression lhs, IExpression rhs, IType rettype)
        {
            Lhs = lhs;
            Rhs = rhs;
            Type = rettype;
        }

        public IType Type
        {
            get;
            private set;
        }

        public IStatement Bind(ref Scope scope)
        {
            return (IStatement)this.Bind(scope);
        }

        public IExpression Bind(Scope scope)
        {
            var boundLhs = Lhs.Bind(scope);
            var boundRhs = Rhs.Bind(scope);

            if (boundLhs.Type.DeTuple() is BoundFunctionLiteral.BoundFunctionType)
            {
                scope.BindFuncLiterals(boundLhs.Type.DeTuple() as BoundFunctionLiteral.BoundFunctionType, boundRhs);
                return new FunctionCall(boundLhs, boundRhs, (boundLhs.Type.DeTuple() as BoundFunctionLiteral.BoundFunctionType).ReturnType(boundRhs));
            }
            throw new Exception("not a function!");
        }
    }

    public class FunctionLiteral : IExpression
    {
        public readonly List<IStatement> Statements;
        public readonly MultiVariableDeclaration Parameter;
        public readonly MultiVariableDeclaration ReturnValue;

        public FunctionLiteral(MultiVariableDeclaration returnValue, MultiVariableDeclaration parameter, List<IStatement> statements)
        {
            this.Type = new FunctionType(parameter.Type, returnValue.Type);
            this.Statements = statements;
            this.Parameter = parameter;
            this.ReturnValue = returnValue;
        }

        public IType Type
        {
            get;
            private set;
        }

        public IExpression Bind(Scope scope)
        {
            BoundFunctionLiteral bfl = new BoundFunctionLiteral(this, scope);
            scope.RegisterFuncLiteral(bfl);
            return bfl;
        }
    }


    public class BoundFunctionLiteral : IExpression
    {
        FunctionLiteral prototype;
        Scope scopeAtPrototype;
        Dictionary<string, FunctionLiteral> boundFunctions = new Dictionary<string, FunctionLiteral>();
        public BoundFunctionLiteral(FunctionLiteral prototype, Scope scopeAtPrototype)
        {
            this.prototype = prototype;
            this.scopeAtPrototype = scopeAtPrototype;
        }

        public class BoundFunctionType : IType
        {
            BoundFunctionLiteral bfl;

            public BoundFunctionType(BoundFunctionLiteral bfl)
            {
                this.bfl = bfl;
            }

            public bool Accepts(IType rtype)
            {
                return bfl.prototype.Type.Accepts(rtype);
            }

            public string Name
            {
                get { return bfl.prototype.Type.Name; }
            }

            public IType ReturnType(IExpression rhs)
            {
                return bfl.boundFunctions[rhs.Type.Name].ReturnValue.Type;
            }

            public IType DeTuple()
            {
                return this;
            }
        }

        public FunctionLiteral GetFunctionFor(Scope scope, IExpression rhs)
        {
            FunctionLiteral fl;
            if (boundFunctions.TryGetValue(rhs.Type.Name, out fl))
            {
                return fl;
            }

            // create a function literal that fits the rhs

            List<IStatement> statements = new List<IStatement>();

            scope = new Scope(scopeAtPrototype);

            MultiVariableDeclaration parms = new MultiVariableDeclaration(prototype.Parameter.Declarations.Select(x => new VariableDeclaration(x.Name, x.Type)).ToList(), rhs);

            for (int i = 0; i < parms.Declarations.Count; i++)
            {
                scope = new Scope(scope, parms.Declarations[i]);
            }

            foreach (var stmt in prototype.Statements)
            {
                statements.Add(stmt.Bind(ref scope));
            }

            List<VariableDeclaration> rets = new List<VariableDeclaration>();

            for (int i = 0; i < prototype.ReturnValue.Declarations.Count; i++)
            {
                string name = prototype.ReturnValue.Declarations[i].Name;
                VariableDeclaration decl = (VariableDeclaration)scope.GetDeclarationForOrNull(name);
                if (decl == null)
                {
                    rets.Add(new VariableDeclaration(name, new TupleType(new List<IType>())));
                }
                else
                {
                    rets.Add(new VariableDeclaration(name, decl.Type));
                }
            }

            fl = new FunctionLiteral(new MultiVariableDeclaration(rets, null), parms, statements);

            boundFunctions[rhs.Type.Name] = fl;

            return fl;
        }

        public IType Type
        {
            get { return new BoundFunctionType(this); }
        }

        public IExpression Bind(Scope scope)
        {
            throw new NotImplementedException();
        }
    }

    class ArrayLiteral : IExpression
    {

        List<IExpression> contents;

        public ArrayLiteral(List<IExpression> contents)
        {
            Type = new ArrayType(new UnknownType(), new IntegerType(0));
            this.contents = contents;
            if (contents.Count > 0)
            {
                Type = new ArrayType(contents[0].Type, new IntegerType(contents.Count));
            }
        }

        public IType Type { get; set; }


        public IExpression Bind(Scope scope)
        {
            return new ArrayLiteral(contents.Select(x => x.Bind(scope)).ToList());
        }
    }

    class IntegerRange : IDeclaration, IExpression, ISetMember
    {

        public readonly long min, max;

        public IntegerRange(long min, long max)
        {
            this.min = min;
            this.max = max;
            if (min == max)
            {
                Type = new IntegerType(min);
            }
            else
            {
                Type = new ArrayType(new IntegerType(min, max), new IntegerType(max - min + 1));
            }
        }

        public IType Type
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                return min + ".." + max;
            }
        }

        public IExpression Bind(Scope scope)
        {
            return new IntegerRange(min, max);
        }
    }

    //class SymbolType : IType
    //{

    //    public SymbolType(string name)
    //    {
    //        this.Name = name;
    //    }

    //    public bool Accepts(IType rtype)
    //    {
    //        return rtype.ToString() == rtype.ToString();
    //    }

    //    public string Name
    //    {
    //        get;
    //        private set;
    //    }

    //    public override string ToString()
    //    {
    //        return "symbol:" + Name;
    //    }
    //}

    class Symbol : IDeclaration, IExpression, ISetMember
    {

        public Symbol(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "symbol:" + Name;
        }

        public IType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IExpression Bind(Scope scope)
        {
            return this;
        }
    }

    class TupleType : IType
    {

        public IType DeTuple()
        {
            if (types.Count == 1)
            {
                return types[0].DeTuple();
            }
            return this;
        }

        public readonly List<IType> types;

        public TupleType(List<IType> types)
        {
            this.types = types;
        }

        public bool Accepts(IType type)
        {
            if (types.Count == 1)
            {
                // special case.
                return types[0].Accepts(type);
            }
            if (type is TupleType)
            {
                if (types.Count == 0)
                {
                    return (type as TupleType).types.Count == 0;
                }
                TupleType other = (TupleType)type;
                // This (receiving tuple) must be shorter than the tuple assigning in
                for (int i = 0; i < types.Count; i++)
                {
                    if (other.types.Count == i || !types[i].Accepts(other.types[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public string Name
        {
            get
            {
                if (types.Count == 1)
                {
                    // special case.
                    return types[0].Name;
                }
                return "(" + string.Join(",", types.Select(x => x.Name)) + ")";
            }
        }

        public override string ToString()
        {
            if (types.Count == 1)
            {
                // special case.
                return types[0].ToString();
            }
            return "tuple:" + Name;
        }
    }

    class TupleExpression : IExpression
    {

        List<IExpression> expressions;

        public TupleExpression(List<IExpression> exprs)
        {
            expressions = exprs;
            Type = new TupleType(exprs.Select(x => x.Type).ToList());
        }

        public IType Type
        {
            get;
            private set;
        }

        public IExpression Bind(Scope scope)
        {
            return new TupleExpression(expressions.Select(x => x.Bind(scope)).ToList());
        }
    }

    public class VariableDeclaration : IExpression, IDeclaration, IVariable
    {

        public IType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public MultiVariableDeclaration MVD
        {
            get;
            set;
        }

        public VariableDeclaration(string name, IType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public override string ToString()
        {
            return "var:" + Name;
        }

        public static VariableDeclaration Find(ref Scope scope, VariableDeclaration vardecl, string name, bool isDeclaration)
        {
            if (vardecl != null)
            {
                return vardecl;
            }

            IDeclaration decl = scope.GetDeclarationForOrNull(name);
            if (decl == null && isDeclaration)
            {
                var vd = new VariableDeclaration(name, new UnknownType());
                scope = new Scope(scope, vd);
                return vd;
            }
            if (decl is VariableDeclaration)
            {
                return (VariableDeclaration)decl;
            }
            throw new Exception(name + " not a variable");
        }

        public IExpression Bind(Scope scope)
        {
            return (IExpression)scope.GetDeclarationFor(Name);
        }
    }

    public class MultiVariableDeclaration : IStatement, IDeclaration, IVariable
    {
        public readonly List<VariableDeclaration> Declarations = new List<VariableDeclaration>();
        public readonly IExpression initializer;

        public MultiVariableDeclaration(List<VariableDeclaration> declarations, IExpression rexp)
        {
            this.Declarations = declarations;
            this.Type = new TupleType(declarations.Select(x =>
            {
                x.MVD = this;
                return x.Type;
            }).ToList());

            if (rexp != null)
            {
                if (!Type.Accepts(rexp.Type))
                {
                    throw new Exception("var type: " + Type + " does not accept: " + rexp.Type);
                }

                // closer checks
                List<IType> types = new List<IType>();
                if (rexp.Type is TupleType)
                {
                    types = new List<IType>((rexp.Type as TupleType).types);
                }
                else
                {
                    types.Add(rexp.Type);
                }

                if (declarations.Count == 1)
                {
                    declarations[0].Type = new TupleType(types);
                }
                else
                {
                    for (int i = 0; i < types.Count; i++)
                    {
                        declarations[i].Type = types[i];
                    }
                }

                this.Type = new TupleType(types);
            }
            this.initializer = rexp;
        }

        public string Name
        {
            get { return string.Join(",", Declarations.Select(x => x.Name)); }
        }

        public override string ToString()
        {
            return "multivar:" + Name;
        }

        public IType Type
        {
            get;
            private set;
        }

        public IStatement Bind(ref Scope scope)
        {
            IExpression boundExpr = initializer.Bind(scope);

            List<VariableDeclaration> newDeclarations = new List<VariableDeclaration>();
            if (boundExpr is TupleType)
            {
                var types = boundExpr as TupleType;
                for (int i = 0; i < Declarations.Count; i++)
                {
                    var vardecl = VariableDeclaration.Find(ref scope, null, Declarations[i].Name, true);
                    newDeclarations.Add(vardecl);
                }
            }
            else
            {
                var vardecl = VariableDeclaration.Find(ref scope, null, Declarations[0].Name, true);
                newDeclarations.Add(vardecl);
            }

            var boundVar = new MultiVariableDeclaration(newDeclarations, boundExpr);

            return boundVar;
        }

        public IExpression Bind(Scope scope)
        {
            throw new Exception();
        }
    }

    class SetDeclaration : IStatement, IType, IDeclaration
    {
        public SetDeclaration(string name, IEnumerable<ISetMember> elements)
        {
            foreach (var ele in elements)
            {
                theset.Add(ele.Name, ele);
                if (ele is Symbol)
                {
                    (ele as Symbol).Type = this;
                }
            }
            this.Name = name;
        }

        public override string ToString()
        {
            return "set:" + Name;
        }

        public string Name
        {
            get;
            set;
        }

        Dictionary<string, ISetMember> theset = new Dictionary<string, ISetMember>();

        public bool Accepts(IType type)
        {
            if (type.ToString() == this.ToString())
            {
                return true;
            }
            // only accepts members
            foreach (var kvpair in theset)
            {
                if (kvpair.Value is IntegerRange)
                {
                    if (type is IntegerType)
                    {
                        if ((type as IntegerType).IsWithin(kvpair.Value as IntegerRange))
                        {
                            return true;
                        }
                    }

                }
                else if (kvpair.Value is Symbol)
                {
                    if (type.ToString() == kvpair.Value.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IStatement Bind(ref Scope scope)
        {
            // This thing would be full declared
            scope = new Scope(scope, this);
            return this;
        }

        public IType DeTuple()
        {
            return this;
        }
    }

    public class Scope
    {
        readonly Scope parent;
        readonly string name;
        readonly Dictionary<string, List<BoundFunctionLiteral>> funcTypeToLiterals;
        readonly Dictionary<string, IDeclaration> scopeTable;
        readonly Dictionary<string, IType> typeTable;

        public Scope()
        {
            this.scopeTable = new Dictionary<string, IDeclaration>();
            this.typeTable = new Dictionary<string, IType>();
            this.funcTypeToLiterals = new Dictionary<string, List<BoundFunctionLiteral>>();
        }

        public Scope(Scope parent)
        {
            this.scopeTable = new Dictionary<string, IDeclaration>(parent.scopeTable);
            this.typeTable = new Dictionary<string, IType>(parent.typeTable);
            this.funcTypeToLiterals = new Dictionary<string, List<BoundFunctionLiteral>>(parent.funcTypeToLiterals);
        }

        public Scope(Scope parent, IDeclaration decl)
        {
            this.name = decl.Name;
            this.parent = parent;
            this.scopeTable = parent.scopeTable;
            this.funcTypeToLiterals = parent.funcTypeToLiterals;
           
            if (scopeTable.ContainsKey(decl.Name))
            {
                throw new Exception("already has " + decl);
            }
            this.typeTable = parent.typeTable;
            scopeTable.Add(decl.Name, decl);
            if (decl is IType)
            {
                this.typeTable.Add(decl.Name, (IType)decl);
            }
        }

        public void RegisterFuncLiteral(BoundFunctionLiteral literal)
        {
            List<BoundFunctionLiteral> list;
            if (!funcTypeToLiterals.TryGetValue(literal.Type.Name, out list))
            {
                list = new List<BoundFunctionLiteral>();
                funcTypeToLiterals[literal.Type.Name] = list;
            }
            list.Add(literal);
        }

        public void BindFuncLiterals(BoundFunctionLiteral.BoundFunctionType type, IExpression rhs)
        {
            foreach (var lit in funcTypeToLiterals[type.Name])
            {
                lit.GetFunctionFor(this, rhs);
            }
        }

        public IDeclaration GetDeclarationFor(string ident)
        {
            IDeclaration decl = GetDeclarationForOrNull(ident);
            if (decl != null)
            {
                return decl;
            }
            throw new Exception(ident + " not found");
        }

        public IDeclaration GetDeclarationForOrNull(string ident)
        {
            IDeclaration decl;
            if (scopeTable.TryGetValue(ident, out decl))
            {
                return decl;
            }
            return null;
        }

        public IType GetTypeDeclaration(string type)
        {
            return typeTable[type];
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Parser p = new Parser(new Scanner(@"D:\VS\Experiments\Eureka\Eureka\test2.is"));
            p.Parse();

            List<IStatement> boundStatements = new List<IStatement>();
            Scope s = new Scope();

            foreach (var statement in p.statements)
            {
                boundStatements.Add(statement.Bind(ref s));
            }

            Console.ReadKey();
        }
    }
}
