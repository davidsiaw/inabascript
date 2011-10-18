using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

    public class Identifier : IExpression {

		internal Identifier(string ident, Scope symbolsInScope) {
			Name = ident;

            ISymbol symbol;
            if (!symbolsInScope.IdentifierDeclared(ident, out symbol))
            {
                string errmsg = string.Format("\"{0}\" not defined!", ident);
                InabaScriptSource.Error(errmsg);
            }
            Type = symbol.Type;
		}

        public string Name { get; private set; }



        public IType Type
        {
            get;
            private set;
        }
    }
}
