using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

    public class Identifier : IExpression {

		internal Identifier(string ident, Scope symbolsInScope) {
			Name = ident;

            if (!symbolsInScope.IdentifierDeclared(ident))
            {
                string errmsg = string.Format("\"{0}\" not defined!", ident);
                InabaScriptSource.Error(errmsg);
            }
		}

        public string Name { get; private set; }


    }
}
