using System.Collections.Generic;
using InabaScript;
using System.Diagnostics;
using StatementList = System.Collections.Generic.List<InabaScript.IStatement>;
using VarList = System.Collections.Generic.List<InabaScript.VariableDeclaration>;
using ExpressionList = System.Collections.Generic.List<InabaScript.IExpression>;



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
	public const int _arrow = 8;
	public const int maxT = 25;

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

bool IsPartOfType()
{
	Token next = scanner.Peek();
	return la.kind == _arrow && next.kind != _ident;
}

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

	
	void IntegerRange(Scope scope, out IntegerRange range) {
		Expect(3);
		long min = long.Parse(t.val); long max = long.Parse(t.val); 
		if (la.kind == 9) {
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
		Expect(10);
		List<IExpression> innerExprs = new List<IExpression>(); 
		if (StartOf(1)) {
			Expression(scope, out innerExpr);
			innerExprs.Add(innerExpr); 
			while (la.kind == 11) {
				Get();
				Expression(scope, out innerExpr);
				innerExprs.Add(innerExpr); 
			}
		}
		Expect(12);
		expr = new ArrayLiteral(innerExprs); 
	}

	void Expression(Scope scope, out IExpression expr) {
		expr = null; 
		scope = new Scope(scope); 
		if (la.kind == 3) {
			IntegerLiteral(scope, out expr);
		} else if (la.kind == 10) {
			ArrayLiteral(scope, out expr);
		} else if (la.kind == 1 || la.kind == 13) {
			Primary(scope, out expr);
		} else SynErr(26);
	}

	void Primary(Scope scope, out IExpression expr) {
		IType type = new UnknownType(); 
		string name = ""; 
		expr = null; 
		IExpression rhs; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
			if (la.kind == 7 || la.kind == 8) {
				if (la.kind == 7) {
					Get();
					TypeIdentifier(scope, out type);
					VariableDeclaration vd = new VariableDeclaration(name, type); 
					MultiVariableDeclaration parmDecl = new MultiVariableDeclaration(new List<VariableDeclaration>(new VariableDeclaration[] {vd}), null); 
					FunctionReturnDefinition(scope, out expr, parmDecl);
				} else {
					VariableDeclaration vd = new VariableDeclaration(name, type); 
					MultiVariableDeclaration parmDecl = new MultiVariableDeclaration(new List<VariableDeclaration>(new VariableDeclaration[] {vd}), null); 
					FunctionReturnDefinition(scope, out expr, parmDecl);
				}
			}
			if (StartOf(1)) {
				expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
				Expression(scope, out rhs);
				expr = new FunctionCall(expr, rhs); 
			}
			expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
		} else if (la.kind == 13) {
			Get();
			VarList vl = new VarList(); 
			if (la.kind == 1 || la.kind == 3 || la.kind == 10) {
				StillPrimary(scope, out expr, vl);
			} else if (la.kind == 14) {
				Get();
			} else SynErr(27);
		} else SynErr(28);
	}

	void TypeIdentifier(Scope scope, out IType type) {
		type = null; 
		FirstOrderTypeIdentifier(scope, out type);
		if (IsPartOfType()) {
			IType returnType; 
			Expect(8);
			TypeIdentifier(scope, out returnType);
			type = new FunctionType(type, returnType); 
		}
	}

	void FunctionReturnDefinition(Scope scope, out IExpression expr, MultiVariableDeclaration parmDecl) {
		MultiVariableDeclaration retDecl; 
		Scope funcScope = new Scope(scope); 
		StatementList stmts = new StatementList(); 
		Expect(8);
		VariableDeclaration.Scopify(ref funcScope, parmDecl.Declarations); 
		VarNomination(ref funcScope, out retDecl, true);
		Expect(15);
		while (StartOf(2)) {
			Statement(ref funcScope, stmts);
		}
		Expect(16);
		expr = new FunctionLiteral(retDecl, parmDecl, stmts); 
	}

	void StillPrimary(Scope scope, out IExpression expr, VarList vl) {
		IType type = new UnknownType(); 
		string name = ""; 
		expr = null; 
		IExpression rhs; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
			if (la.kind == 14) {
				Get();
				if (la.kind == 8) {
					VariableDeclaration newestParam = new VariableDeclaration(name, type); 
					vl.Add(newestParam); 
					MultiVariableDeclaration parmDecl = new MultiVariableDeclaration(vl, null); 
					FunctionReturnDefinition(scope, out expr, parmDecl);
				}
				if (StartOf(1)) {
					expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
					Expression(scope, out rhs);
					expr = new FunctionCall(expr, rhs); 
				}
				expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
			} else if (la.kind == 7) {
				Get();
				TypeIdentifier(scope, out type);
				VariableDeclaration newestParam = new VariableDeclaration(name, type); 
				vl.Add(newestParam); 
				MultiVariableDeclaration parmDecl = new MultiVariableDeclaration(vl, null); 
				if (la.kind == 8) {
					FunctionReturnDefinition(scope, out expr, parmDecl);
					if (la.kind == 14) {
						Get();
						if (StartOf(1)) {
							Expression(scope, out rhs);
							expr = new FunctionCall(expr, rhs); 
						}
					} else if (StartOf(1)) {
						Expression(scope, out rhs);
						expr = new FunctionCall(expr, rhs); 
						if (la.kind == 14) {
							Get();
						} else if (la.kind == 11) {
							ExpressionList el = new ExpressionList(); 
							expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
							el.Add(expr); 
							Get();
							Secondary(scope, el, out expr);
						} else SynErr(29);
					} else if (la.kind == 11) {
						ExpressionList el = new ExpressionList(); 
						expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
						el.Add(expr); 
						Get();
						Secondary(scope, el, out expr);
					} else SynErr(30);
				} else if (la.kind == 14) {
					Get();
					FunctionReturnDefinition(scope, out expr, parmDecl);
					if (StartOf(1)) {
						Expression(scope, out rhs);
						expr = new FunctionCall(expr, rhs); 
					}
					if (la.kind == 11) {
						ExpressionList el = new ExpressionList(); 
						expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
						el.Add(expr); 
						Get();
						Secondary(scope, el, out expr);
					}
				} else if (la.kind == 11) {
					VariableDeclaration vd; 
					NextNomination(ref scope, out vd);
					FunctionReturnDefinition(scope, out expr, parmDecl);
					if (StartOf(1)) {
						Expression(scope, out rhs);
						expr = new FunctionCall(expr, rhs); 
					}
				} else SynErr(31);
			} else if (la.kind == 11) {
				Get();
				vl.Add(new VariableDeclaration(name, type)); 
				StillPrimary(scope, out expr, vl);
			} else if (la.kind == 8) {
				VariableDeclaration newestParam = new VariableDeclaration(name, type); 
				vl.Add(newestParam); 
				MultiVariableDeclaration parmDecl = new MultiVariableDeclaration(vl, null); 
				FunctionReturnDefinition(scope, out expr, parmDecl);
				if (la.kind == 14) {
					Get();
					Expression(scope, out rhs);
					expr = new FunctionCall(expr, rhs); 
				} else if (StartOf(3)) {
					if (la.kind == 11) {
						Get();
					}
					ExpressionList el = new ExpressionList(); 
					expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
					el.Add(expr); 
					Secondary(scope, el, out expr);
				} else SynErr(32);
			} else SynErr(33);
		} else if (la.kind == 3 || la.kind == 10) {
			if (la.kind == 3) {
				IntegerLiteral(scope, out expr);
			} else {
				ArrayLiteral(scope, out expr);
			}
			ExpressionList el = new ExpressionList(); 
			expr = expr ?? (IExpression)scope.GetDeclarationFor(name); 
			el.Add(expr); 
			if (la.kind == 11) {
				Get();
				Secondary(scope, el, out expr);
			}
		} else SynErr(34);
	}

	void Secondary(Scope scope, ExpressionList el, out IExpression expr) {
		Expression(scope, out expr);
		el.Add(expr); 
		if (la.kind == 11) {
			Get();
			Secondary(scope, el, out expr);
		} else if (la.kind == 14) {
			Get();
			expr = new TupleExpression(el); 
		} else SynErr(35);
	}

	void NextNomination(ref Scope scope, out VariableDeclaration varDecl) {
		Expect(11);
		SingleVarNomination(ref scope, out varDecl, true);
		if (la.kind == 11) {
			NextNomination(ref scope, out varDecl);
		} else if (la.kind == 14) {
			Get();
		} else SynErr(36);
	}

	void SingleVarNomination(ref Scope scope, out VariableDeclaration varDecl, bool decl) {
		IType type = new UnknownType(); 
		varDecl = null; 
		Expect(1);
		string name = t.val; 
		if (la.kind == 7) {
			Get();
			TypeIdentifier(scope, out type);
			varDecl = new VariableDeclaration(name, type); 
			scope = new Scope(scope, varDecl); 
		}
		varDecl = varDecl ?? VariableDeclaration.Find(ref scope, varDecl, name, decl); 
	}

	void VarNomination(ref Scope scope, out MultiVariableDeclaration mvd, bool decl) {
		VariableDeclaration varDecl; 
		List<VariableDeclaration> vars = new List<VariableDeclaration>(); 
		if (la.kind == 1) {
			SingleVarNomination(ref scope, out varDecl, decl);
			vars.Add(varDecl); 
		} else if (la.kind == 13) {
			Get();
			SingleVarNomination(ref scope, out varDecl, decl);
			vars.Add(varDecl); 
			while (la.kind == 11) {
				Get();
				SingleVarNomination(ref scope, out varDecl, decl);
				vars.Add(varDecl); 
			}
			Expect(14);
		} else SynErr(37);
		mvd = new MultiVariableDeclaration(vars, null); 
	}

	void Statement(ref Scope scope, StatementList stmts) {
		IStatement stmt; 
		if (la.kind == 22) {
			SetDeclaration(ref scope, out stmt);
			stmts.Add(stmt); 
		} else if (la.kind == 1 || la.kind == 13 || la.kind == 21) {
			VarDeclaration(ref scope, out stmt);
			stmts.Add(stmt); 
		} else SynErr(38);
		Expect(24);
		while (!(StartOf(4))) {SynErr(39); Get();}
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
		Expect(17);
		Expect(15);
		IntegerRangeAsType(scope, out intType);
		type = intType; 
		Expect(16);
	}

	void TupleType(Scope scope, out IType type) {
		IType innerType; 
		Expect(13);
		List<IType> types = new List<IType>(); 
		if (StartOf(5)) {
			TypeIdentifier(scope, out innerType);
			types.Add(innerType); 
			while (la.kind == 11) {
				Get();
				TypeIdentifier(scope, out innerType);
				types.Add(innerType); 
			}
		}
		Expect(14);
		type = new TupleType(types); 
	}

	void ArrayType(Scope scope, out IType type) {
		IType innerType; 
		IntegerType sizeType; 
		Expect(10);
		TypeIdentifier(scope, out innerType);
		Expect(18);
		IntegerRangeAsType(scope, out sizeType);
		Expect(19);
		Expect(12);
		type = new ArrayType(innerType, sizeType); 
	}

	void FirstOrderTypeIdentifier(Scope scope, out IType type) {
		type = null; 
		if (la.kind == 13) {
			TupleType(scope, out type);
		} else if (la.kind == 17) {
			IntegerTypeDeclaration(scope, out type);
		} else if (la.kind == 2) {
			LiteralType(scope, out type);
		} else if (la.kind == 10) {
			ArrayType(scope, out type);
		} else SynErr(40);
	}

	void Initializer(Scope scope, out IExpression expr) {
		Expect(20);
		Expression(scope, out expr);
	}

	void VarAssignment(ref Scope scope, out IStatement stmt) {
		MultiVariableDeclaration mvd; 
		IExpression expr = null; 
		IExpression rhs = null; 
		VariableDeclaration vd = null; 
		stmt = null; 
		if (la.kind == 21) {
			Get();
			VarNomination(ref scope, out mvd, true);
			Initializer(scope, out expr);
			stmt = new MultiVariableDeclaration(mvd.Declarations, expr); 
		} else if (la.kind == 1) {
			SingleVarNomination(ref scope, out vd, false);
			if (la.kind == 20) {
				Initializer(scope, out expr);
				stmt = new MultiVariableDeclaration(new List<VariableDeclaration>(new VariableDeclaration[]{ vd }), expr); 
			} else if (StartOf(1)) {
				Expression(scope, out rhs);
				stmt = new FunctionCall(vd, rhs); 
			} else SynErr(41);
		} else if (la.kind == 1 || la.kind == 13) {
			VarNomination(ref scope, out mvd, false);
			Initializer(scope, out expr);
			stmt = new MultiVariableDeclaration(mvd.Declarations, expr); 
		} else SynErr(42);
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
		Expect(22);
		Expect(2);
		string name = t.val; 
		Expect(20);
		SetSymbol(ref scope, out symbol);
		elements.Add(symbol); 
		scope = new Scope(scope, (IDeclaration)symbol); 
		while (la.kind == 23) {
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
		while (StartOf(2)) {
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
		{T,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,T,T,x, x,x,x},
		{x,T,x,T, x,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x},
		{x,T,x,T, x,x,x,x, x,x,T,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{T,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,T,T,x, x,x,x},
		{x,x,T,x, x,x,x,x, x,x,T,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x}

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
			case 8: s = "arrow expected"; break;
			case 9: s = "\"..\" expected"; break;
			case 10: s = "\"[\" expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\"]\" expected"; break;
			case 13: s = "\"(\" expected"; break;
			case 14: s = "\")\" expected"; break;
			case 15: s = "\"{\" expected"; break;
			case 16: s = "\"}\" expected"; break;
			case 17: s = "\"Int\" expected"; break;
			case 18: s = "\"<\" expected"; break;
			case 19: s = "\">\" expected"; break;
			case 20: s = "\"=\" expected"; break;
			case 21: s = "\"var\" expected"; break;
			case 22: s = "\"set\" expected"; break;
			case 23: s = "\"|\" expected"; break;
			case 24: s = "\";\" expected"; break;
			case 25: s = "??? expected"; break;
			case 26: s = "invalid Expression"; break;
			case 27: s = "invalid Primary"; break;
			case 28: s = "invalid Primary"; break;
			case 29: s = "invalid StillPrimary"; break;
			case 30: s = "invalid StillPrimary"; break;
			case 31: s = "invalid StillPrimary"; break;
			case 32: s = "invalid StillPrimary"; break;
			case 33: s = "invalid StillPrimary"; break;
			case 34: s = "invalid StillPrimary"; break;
			case 35: s = "invalid Secondary"; break;
			case 36: s = "invalid NextNomination"; break;
			case 37: s = "invalid VarNomination"; break;
			case 38: s = "invalid Statement"; break;
			case 39: s = "this symbol not expected in Statement"; break;
			case 40: s = "invalid FirstOrderTypeIdentifier"; break;
			case 41: s = "invalid VarAssignment"; break;
			case 42: s = "invalid VarAssignment"; break;

			default: s = "error " + n; break;
		}
		throw new Exception(s);
		// errorStream.WriteLine(errMsgFormat, line, col, s);
		// count++;
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
