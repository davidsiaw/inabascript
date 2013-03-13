using System.Collections.Generic;
using InabaScript;
using StatementList = System.Collections.Generic.List<InabaScript.IStatement>;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _typeident = 2;
	public const int _integer = 3;
	public const int _float = 4;
	public const int _utf8bom = 5;
	public const int _validStringLiteral = 6;
	public const int _colon = 7;
	public const int maxT = 26;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public Scope endScope = new Scope();
public StatementList statements = new StatementList();

/* If you want your generated compiler case insensitive add the */
/* keyword IGNORECASE here. */



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void SymbolLiteral(Scope scope, out IExpression expr) {
		Expect(1);
		expr = (IExpression)scope.GetDeclarationFor(t.val); 
	}

	void TupleType(Scope scope, out IType type) {
		IType innerType; 
		Expect(8);
		List<IType> types = new List<IType>(); 
		if (StartOf(1)) {
			TypeIdentifier(scope, out innerType);
			types.Add(innerType); 
			while (la.kind == 9) {
				Get();
				TypeIdentifier(scope, out innerType);
				types.Add(innerType); 
			}
		}
		Expect(10);
		type = new TupleType(types); 
	}

	void TypeIdentifier(Scope scope, out IType type) {
		type = null; 
		if (la.kind == 8) {
			TupleType(scope, out type);
		} else if (la.kind == 18) {
			IntegerTypeDeclaration(scope, out type);
		} else if (la.kind == 2) {
			LiteralType(scope, out type);
		} else if (la.kind == 12) {
			ArrayType(scope, out type);
		} else SynErr(27);
		if (la.kind == 15) {
			IType returnType; 
			Get();
			TypeIdentifier(scope, out returnType);
			type = new FunctionType(type, returnType); 
		}
	}

	void TupleLiteral(Scope scope, out IExpression expr) {
		IExpression innerExpression; 
		Expect(8);
		List<IExpression> exprs = new List<IExpression>(); 
		if (StartOf(2)) {
			Expression(scope, out innerExpression);
			exprs.Add(innerExpression); 
			while (la.kind == 9) {
				Get();
				Expression(scope, out innerExpression);
				exprs.Add(innerExpression); 
			}
		}
		Expect(10);
		expr = new TupleExpression(exprs); 
	}

	void Expression(Scope scope, out IExpression expr) {
		expr = null; 
		if (la.kind == 14) {
			FunctionLiteral(scope, out expr);
		} else if (la.kind == 8) {
			TupleLiteral(scope, out expr);
		} else if (la.kind == 1) {
			SymbolLiteral(scope, out expr);
		} else if (la.kind == 3) {
			IntegerLiteral(scope, out expr);
		} else if (la.kind == 12) {
			ArrayLiteral(scope, out expr);
		} else SynErr(28);
		if (StartOf(2)) {
			IExpression rhs; 
			Expression(scope, out rhs);
			expr = new FunctionCall(expr, rhs); 
		}
	}

	void IntegerRange(Scope scope, out IntegerRange range) {
		Expect(3);
		long min = long.Parse(t.val); long max = long.Parse(t.val); 
		if (la.kind == 11) {
			Get();
			Expect(3);
			max = long.Parse(t.val); 
		}
		range = new IntegerRange(min, max); 
	}

	void IntegerLiteral(Scope scope, out IExpression expr) {
		IntegerRange range; 
		IntegerRange(scope, out range);
		expr = range; 
	}

	void ArrayLiteral(Scope scope, out IExpression expr) {
		IExpression innerExpr; 
		Expect(12);
		List<IExpression> innerExprs = new List<IExpression>(); 
		if (StartOf(2)) {
			Expression(scope, out innerExpr);
			innerExprs.Add(innerExpr); 
			while (la.kind == 9) {
				Get();
				Expression(scope, out innerExpr);
				innerExprs.Add(innerExpr); 
			}
		}
		Expect(13);
		expr = new ArrayLiteral(innerExprs); 
	}

	void FunctionLiteral(Scope scope, out IExpression expr) {
		MultiVariableDeclaration retDecl; 
		MultiVariableDeclaration parmDecl; 
		Scope funcScope = new Scope(scope); 
		List<IStatement> stmts = new List<IStatement>(); 
		Expect(14);
		VarNomination(ref funcScope, out parmDecl, true);
		Expect(15);
		VarNomination(ref funcScope, out retDecl, true);
		Expect(16);
		while (StartOf(3)) {
			Statement(ref funcScope, stmts);
		}
		Expect(17);
		expr = new FunctionLiteral(retDecl, parmDecl, stmts); 
	}

	void VarNomination(ref Scope scope, out MultiVariableDeclaration mvd, bool decl) {
		VariableDeclaration varDecl; 
		List<VariableDeclaration> vars = new List<VariableDeclaration>(); 
		if (la.kind == 1) {
			SingleVarNomination(ref scope, out varDecl, decl);
			vars.Add(varDecl); 
		} else if (la.kind == 8) {
			Get();
			SingleVarNomination(ref scope, out varDecl, decl);
			vars.Add(varDecl); 
			while (la.kind == 9) {
				Get();
				SingleVarNomination(ref scope, out varDecl, decl);
				vars.Add(varDecl); 
			}
			Expect(10);
		} else SynErr(29);
		mvd = new MultiVariableDeclaration(vars, null); 
	}

	void Statement(ref Scope scope, StatementList stmts) {
		IStatement stmt; 
		if (la.kind == 23) {
			SetDeclaration(ref scope, out stmt);
			stmts.Add(stmt); 
		} else if (la.kind == 1 || la.kind == 8 || la.kind == 22) {
			VarDeclaration(ref scope, out stmt);
			stmts.Add(stmt); 
		} else SynErr(30);
		Expect(25);
		while (!(StartOf(4))) {SynErr(31); Get();}
	}

	void LiteralType(Scope scope, out IType type) {
		Expect(2);
		type = scope.GetTypeDeclaration(t.val); 
	}

	void IntegerRangeAsType(Scope scope, out IntegerType type) {
		IntegerRange range; 
		IntegerRange(scope, out range);
		type = new IntegerType(range.min, range.max); 
	}

	void IntegerTypeDeclaration(Scope scope, out IType type) {
		IntegerType intType; 
		Expect(18);
		Expect(16);
		IntegerRangeAsType(scope, out intType);
		type = intType; 
		Expect(17);
	}

	void ArrayType(Scope scope, out IType type) {
		IType innerType; 
		IntegerType sizeType; 
		Expect(12);
		TypeIdentifier(scope, out innerType);
		Expect(19);
		IntegerRangeAsType(scope, out sizeType);
		Expect(20);
		Expect(13);
		type = new ArrayType(innerType, sizeType); 
	}

	void SingleVarNomination(ref Scope scope, out VariableDeclaration varDecl, bool decl) {
		IType type = new UnknownType(); 
		varDecl = null; 
		Expect(1);
		string name = t.val; 
		if (la.kind == 7) {
			Get();
			TypeIdentifier(scope, out type);
		}
		varDecl = VariableDeclaration.Find(ref scope, varDecl, name, decl); 
	}

	void Initializer(Scope scope, out IExpression expr) {
		Expect(21);
		Expression(scope, out expr);
	}

	void VarAssignment(ref Scope scope, out IStatement stmt) {
		MultiVariableDeclaration mvd; 
		IExpression expr = null; 
		IExpression rhs = null; 
		VariableDeclaration vd = null; 
		stmt = null; 
		if (la.kind == 22) {
			Get();
			VarNomination(ref scope, out mvd, true);
			Initializer(scope, out expr);
			stmt = new MultiVariableDeclaration(mvd.Declarations, expr); 
		} else if (la.kind == 1) {
			SingleVarNomination(ref scope, out vd, false);
			if (la.kind == 21) {
				Initializer(scope, out expr);
				stmt = new MultiVariableDeclaration(new List<VariableDeclaration>(new VariableDeclaration[]{ vd }), expr); 
			} else if (StartOf(2)) {
				Expression(scope, out rhs);
				stmt = new FunctionCall(vd, rhs); 
			} else SynErr(32);
		} else if (la.kind == 1 || la.kind == 8) {
			VarNomination(ref scope, out mvd, false);
			Initializer(scope, out expr);
			stmt = new MultiVariableDeclaration(mvd.Declarations, expr); 
		} else SynErr(33);
	}

	void VarDeclaration(ref Scope scope, out IStatement stmt) {
		stmt = null; 
		bool decl = false; 
		VarAssignment(ref scope, out stmt);
	}

	void SetSymbol(ref Scope scope, out ISetMember elem) {
		elem = null; 
		Expect(1);
		elem = new Symbol(t.val); 
	}

	void SetDeclaration(ref Scope scope, out IStatement stmt) {
		List<ISetMember> elements = new List<ISetMember>(); 
		ISetMember symbol; 
		Expect(23);
		Expect(2);
		string name = t.val; 
		Expect(21);
		SetSymbol(ref scope, out symbol);
		elements.Add(symbol); 
		scope = new Scope(scope, (IDeclaration)symbol); 
		while (la.kind == 24) {
			Get();
			SetSymbol(ref scope, out symbol);
			elements.Add(symbol); 
			scope = new Scope(scope, (IDeclaration)symbol); 
		}
		SetDeclaration set = new SetDeclaration(name, elements); 
		stmt = set; 
		scope = new Scope(scope, set); 
	}

	void InabaScript() {
		
		if (la.kind == 5) {
			Get();
		}
		while (StartOf(3)) {
			Statement(ref endScope, statements);
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		InabaScript();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,T, x,x,x,x},
		{x,x,T,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x},
		{x,T,x,T, x,x,x,x, T,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x},
		{T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,T, x,x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "typeident expected"; break;
			case 3: s = "integer expected"; break;
			case 4: s = "float expected"; break;
			case 5: s = "utf8bom expected"; break;
			case 6: s = "validStringLiteral expected"; break;
			case 7: s = "colon expected"; break;
			case 8: s = "\"(\" expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\")\" expected"; break;
			case 11: s = "\"..\" expected"; break;
			case 12: s = "\"[\" expected"; break;
			case 13: s = "\"]\" expected"; break;
			case 14: s = "\"function\" expected"; break;
			case 15: s = "\"->\" expected"; break;
			case 16: s = "\"{\" expected"; break;
			case 17: s = "\"}\" expected"; break;
			case 18: s = "\"Int\" expected"; break;
			case 19: s = "\"<\" expected"; break;
			case 20: s = "\">\" expected"; break;
			case 21: s = "\"=\" expected"; break;
			case 22: s = "\"var\" expected"; break;
			case 23: s = "\"set\" expected"; break;
			case 24: s = "\"|\" expected"; break;
			case 25: s = "\";\" expected"; break;
			case 26: s = "??? expected"; break;
			case 27: s = "invalid TypeIdentifier"; break;
			case 28: s = "invalid Expression"; break;
			case 29: s = "invalid VarNomination"; break;
			case 30: s = "invalid Statement"; break;
			case 31: s = "this symbol not expected in Statement"; break;
			case 32: s = "invalid VarAssignment"; break;
			case 33: s = "invalid VarAssignment"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
