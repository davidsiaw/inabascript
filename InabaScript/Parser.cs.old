using System.Collections.Generic;

using System;



namespace InabaScript {

    using ExpressionList = List<IExpression>;
    
	internal class Parser {
		const int _EOF = 0;
	const int _ident = 1;
	const int _type = 2;
	const int _integer = 3;
	const int _float = 4;
	const int _utf8bom = 5;
	const int _validStringLiteral = 6;
	const int _colon = 7;
	const int maxT = 23;

		const bool T = true;
		const bool x = false;
		const int minErrDist = 2;

		public static Token t;    // last recognized token
		public static Token la;   // lookahead token
		static int errDist = minErrDist;

	internal static InabaScriptSource iss;
static int anonfunc = 0;
    
/* If you want your generated compiler case insensitive add the */
/* keyword IGNORECASE here. */




		static void SynErr (int n) {
			if (errDist >= minErrDist) Errors.SynErr(la.line, la.col, n);
			errDist = 0;
		}

		public static void SemErr (string msg) {
			if (errDist >= minErrDist) Errors.Error(t.line, t.col, msg);
			errDist = 0;
		}
		
		static void Get () {
			for (;;) {
				t = la;
				la = Scanner.Scan();
				if (la.kind <= maxT) { ++errDist; break; }
	
				la = t;
			}
		}
		
		static void Expect (int n) {
			if (la.kind==n) Get(); else { SynErr(n); }
		}
		
		static bool StartOf (int s) {
			return set[s, la.kind];
		}
		
		static void ExpectWeak (int n, int follow) {
			if (la.kind == n) Get();
			else {
				SynErr(n);
				while (!StartOf(follow)) Get();
			}
		}
		
		static bool WeakSeparator (int n, int syFol, int repFol) {
			bool[] s = new bool[maxT+1];
			if (la.kind == n) { Get(); return true; }
			else if (StartOf(repFol)) return false;
			else {
				for (int i=0; i <= maxT; i++) {
					s[i] = set[syFol, i] || set[repFol, i] || set[0, i];
				}
				SynErr(n);
				while (!s[la.kind]) Get();
				return StartOf(syFol);
			}
		}
		
		static void Identifier(out string ident) {
		Expect(1);
		ident = t.val; 
	}

	static void Type(out string type) {
		Expect(2);
		type = t.val; 
	}

	static void String(out string str) {
		Expect(6);
		str = t.val.Substring(1, t.val.Length - 2); 
	}

	static void FunctionParamsAndBody(out FunctionBody body, Scope scope) {
		string ident; 
		string type = null; 
		IStatement stmt; 
		List<VariableDeclaration> parameters = new List<VariableDeclaration>(); 
		List<IStatement> statements = new List<IStatement>(); 
		VariableDeclaration vd; 
		Expect(8);
		if (la.kind == 1) {
			Identifier(out ident);
			if (la.kind == 7) {
				Get();
				Type(out type);
			}
			vd = new VariableDeclaration(ident, type);
			parameters.Add(vd); 
			scope = new Scope(vd, scope);	
			while (la.kind == 9) {
				Get();
				type = null; 
				Identifier(out ident);
				if (la.kind == 7) {
					Get();
					Type(out type);
				}
				vd = new VariableDeclaration(ident, type);
				parameters.Add(vd); 
				scope = new Scope(vd,scope);	
			}
		}
		Expect(10);
		Expect(11);
		while (StartOf(1)) {
			Statement(out stmt, ref scope);
			statements.Add(stmt); 
		}
		Expect(12);
		body = new FunctionBody(parameters, statements, scope); 
	}

	static void Statement(out IStatement stmt, ref Scope scope) {
		FunctionDeclaration funcdecl; 
		VariableDeclaration vardecl; 
		ReturnStatement retstmt; 
		FunctionCall funccall; 
		stmt = null; 
		if (la.kind == 19) {
			VariableDeclaration(out vardecl, ref scope);
			stmt = vardecl; 
		} else if (la.kind == 13) {
			FunctionDeclaration(out funcdecl, ref scope);
			stmt = funcdecl; 
		} else if (la.kind == 1 || la.kind == 8 || la.kind == 13) {
			FunctionCall(out funccall, ref scope);
			Expect(21);
			stmt = funccall; 
		} else if (la.kind == 22) {
			ReturnStatement(out retstmt, ref scope);
			stmt = retstmt; 
		} else SynErr(24);
	}

	static void FunctionDeclaration(out FunctionDeclaration funcdecl, ref Scope scope) {
		string ident = "anon" + anonfunc ; anonfunc++; 
		Expect(13);
		FunctionBody body; 
		if (la.kind == 1) {
			Identifier(out ident);
		}
		FunctionParamsAndBody(out body, scope);
		funcdecl = new FunctionDeclaration(ident, body); 
		scope = new Scope(funcdecl, scope); 
	}

	static void Expression(out IExpression expr, ref Scope scope) {
		IExpression rhs; 
		string op; 
		Term(out expr, ref scope);
		while (la.kind == 14 || la.kind == 15) {
			if (la.kind == 14) {
				Get();
				op = "(+)"; 
			} else {
				Get();
				op = "(-)"; 
			}
			Term(out rhs, ref scope);
			expr = new FunctionCall(new Identifier(op, scope), expr, rhs); 
		}
	}

	static void Term(out IExpression expr, ref Scope scope) {
		IExpression rhs; 
		string op; 
		Factor(out expr, ref scope);
		while (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
				op = "(*)"; 
			} else {
				Get();
				op = "(/)"; 
			}
			Factor(out rhs, ref scope);
			expr = new FunctionCall(new Identifier(op, scope), expr, rhs); 
		}
	}

	static void Factor(out IExpression expr, ref Scope scope) {
		IExpression rhs; string ident;
		InnerReferencable(out expr, ref scope);
		while (la.kind == 18) {
			Get();
			expr = new InnerReference(expr); 
			Identifier(out ident);
			(expr as InnerReference).SetMemberIdent(ident); 
		}
	}

	static void InnerReferencable(out IExpression expr, ref Scope scope) {
		expr = null; string str = null;
		List<IExpression> callers; 
		ObjectDeclaration obj; 
		if (la.kind == 4) {
			Get();
			expr = new FloatLiteral(t.val); 
		} else if (la.kind == 3) {
			Get();
			expr = new IntegerLiteral(t.val); 
		} else if (la.kind == 6) {
			String(out str);
			expr = new StringLiteral(str); 
		} else if (la.kind == 11) {
			ObjectDeclaration(out obj, ref scope);
			expr = obj; 
		} else if (la.kind == 1 || la.kind == 8 || la.kind == 13) {
			Referencer(out expr, ref scope);
			if (la.kind == 8) {
				Caller(out callers, ref scope);
				expr = new FunctionCall(expr, callers); 
			}
		} else SynErr(25);
	}

	static void ObjectDeclaration(out ObjectDeclaration obj, ref Scope scope) {
		obj = new ObjectDeclaration(); 
		VariableDeclaration vardecl; 
		Expect(11);
		if (la.kind == 1) {
			MemberDeclaration(out vardecl, ref scope);
			obj.AddMember(vardecl); 
			while (la.kind == 9) {
				Get();
				MemberDeclaration(out vardecl, ref scope);
				obj.AddMember(vardecl); 
			}
		}
		Expect(12);
	}

	static void Referencer(out IExpression expr, ref Scope scope) {
		FunctionDeclaration funcdecl; 
		string ident; 
		expr = null; 
		if (la.kind == 8) {
			Get();
			Expression(out expr, ref scope);
			Expect(10);
		} else if (la.kind == 13) {
			FunctionDeclaration(out funcdecl, ref scope);
			expr = funcdecl; 
		} else if (la.kind == 1) {
			Identifier(out ident);
			expr = new Identifier(ident, scope); 
		} else SynErr(26);
	}

	static void Caller(out ExpressionList callers, ref Scope scope) {
		IExpression expr; 
		Expect(8);
		callers = new List<IExpression>(); 
		if (StartOf(2)) {
			Expression(out expr, ref scope);
			callers.Add(expr); 
			while (la.kind == 9) {
				Get();
				Expression(out expr, ref scope);
				callers.Add(expr); 
			}
		}
		Expect(10);
	}

	static void MemberDeclaration(out VariableDeclaration vardecl, ref Scope scope) {
		IExpression expr = null; 
		string ident; 
		Identifier(out ident);
		Expect(7);
		Expression(out expr, ref scope);
		vardecl = new VariableDeclaration(ident, expr); 
	}

	static void VariableDeclaration(out VariableDeclaration vardecl, ref Scope scope) {
		IExpression expr = null; 
		Expect(19);
		string ident; 
		Identifier(out ident);
		if (la.kind == 20) {
			Get();
			Expression(out expr, ref scope);
		}
		Expect(21);
		vardecl = new VariableDeclaration(ident, expr); 
		scope = new Scope(vardecl, scope); 
	}

	static void FunctionCall(out FunctionCall funccall, ref Scope scope) {
		IExpression expr; 
		List<IExpression> callers; 
		Referencer(out expr, ref scope);
		Caller(out callers, ref scope);
		funccall = new FunctionCall(expr, callers); 
	}

	static void ReturnStatement(out ReturnStatement retstmt, ref Scope scope) {
		IExpression expr; 
		Expect(22);
		Expression(out expr, ref scope);
		Expect(21);
		retstmt = new ReturnStatement(expr); 
	}

	static void InabaScript() {
		IStatement stmt; 
		Scope scope = iss.scope; 
		if (la.kind == 5) {
			Get();
		}
		while (StartOf(1)) {
			while (!(StartOf(3))) {SynErr(27); Get();}
			Statement(out stmt, ref scope);
			iss.stmts.Add(stmt); 
		}
		iss.scope = scope; 
	}



		public static void Parse() {
			la = new Token();
			la.val = "";		
			Get();
			InabaScript();

		Expect(0);
		Buffer.Close();
		}

		static bool[,] set = {
			{T,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x},
		{x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x},
		{x,T,x,T, T,x,T,x, T,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x}

		};
	} // end Parser


	internal class Errors {
		public static int count = 0;                                    // number of errors detected
	  public static string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
		
		public static void SynErr (int line, int col, int n) {
			string s;
			switch (n) {
				case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "type expected"; break;
			case 3: s = "integer expected"; break;
			case 4: s = "float expected"; break;
			case 5: s = "utf8bom expected"; break;
			case 6: s = "validStringLiteral expected"; break;
			case 7: s = "colon expected"; break;
			case 8: s = "\"(\" expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\")\" expected"; break;
			case 11: s = "\"{\" expected"; break;
			case 12: s = "\"}\" expected"; break;
			case 13: s = "\"function\" expected"; break;
			case 14: s = "\"+\" expected"; break;
			case 15: s = "\"-\" expected"; break;
			case 16: s = "\"*\" expected"; break;
			case 17: s = "\"/\" expected"; break;
			case 18: s = "\".\" expected"; break;
			case 19: s = "\"var\" expected"; break;
			case 20: s = "\"=\" expected"; break;
			case 21: s = "\";\" expected"; break;
			case 22: s = "\"return\" expected"; break;
			case 23: s = "??? expected"; break;
			case 24: s = "invalid Statement"; break;
			case 25: s = "invalid InnerReferencable"; break;
			case 26: s = "invalid Referencer"; break;
			case 27: s = "this symbol not expected in InabaScript"; break;

				default: s = "error " + n; break;
			}
			Console.WriteLine(Errors.errMsgFormat, line, col, s);
			count++;
		}

		public static void SemErr (int line, int col, int n) {
			Console.WriteLine(errMsgFormat, line, col, ("error " + n));
			count++;
		}

		public static void Error (int line, int col, string s) {
			Console.WriteLine(errMsgFormat, line, col, s);
			count++;
		}

		public static void Exception (string s) {
			Console.WriteLine(s); 
	#if !WindowsCE
			System.Environment.Exit(1);
	#endif
		}
	} // Errors
}

