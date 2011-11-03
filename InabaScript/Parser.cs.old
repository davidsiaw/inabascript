using System.Collections.Generic;

using System;



namespace InabaScript {
    
	internal class Parser {
		const int _EOF = 0;
	const int _ident = 1;
	const int _type = 2;
	const int _integer = 3;
	const int _float = 4;
	const int _utf8bom = 5;
	const int _validStringLiteral = 6;
	const int _colon = 7;
	const int maxT = 11;

		const bool T = true;
		const bool x = false;
		const int minErrDist = 2;

		public static Token t;    // last recognized token
		public static Token la;   // lookahead token
		static int errDist = minErrDist;

	public static InabaScriptSource iss;

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
		
		static void Identifier(out string identifier) {
		Expect(1);
		identifier = t.val; 
	}

	static void IntegerLiteral(out IExpression integer) {
		Expect(3);
		integer = new IntegerLiteral(long.Parse(t.val)); 
	}

	static void Referencer(ref Scope scope, out IExpression expr) {
		string identifier; 
		Identifier(out identifier);
		expr = new Referencer(scope, identifier); 
	}

	static void Expression(ref Scope scope, out IExpression expr) {
		expr = null; 
		if (la.kind == 3) {
			IntegerLiteral(out expr);
		} else if (la.kind == 1) {
			Referencer(ref scope, out expr);
		} else SynErr(12);
	}

	static void VariableDeclaration(ref Scope scope, out IStatement vardecl) {
		string identifier; 
		IExpression expr; 
		Expect(8);
		Identifier(out identifier);
		Expect(9);
		Expression(ref scope, out expr);
		vardecl = new VariableDeclaration(identifier, expr); 
		scope = new Scope(vardecl as VariableDeclaration, scope); 
	}

	static void Statement(ref Scope scope, out IStatement statement) {
		VariableDeclaration(ref scope, out statement);
	}

	static void InabaScript() {
		Scope scope = null; 
		IStatement stmt; 
		if (la.kind == 5) {
			Get();
		}
		while (la.kind == 8) {
			while (!(la.kind == 0 || la.kind == 8)) {SynErr(13); Get();}
			Statement(ref scope, out stmt);
			Expect(10);
			iss.statements.Add(stmt); 
		}
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
			{T,x,x,x, x,x,x,x, T,x,x,x, x}

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
			case 8: s = "\"var\" expected"; break;
			case 9: s = "\"=\" expected"; break;
			case 10: s = "\";\" expected"; break;
			case 11: s = "??? expected"; break;
			case 12: s = "invalid Expression"; break;
			case 13: s = "this symbol not expected in InabaScript"; break;

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

