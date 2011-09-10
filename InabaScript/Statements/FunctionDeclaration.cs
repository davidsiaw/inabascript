using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

	public class FunctionDeclaration : IStatement, IExpression, ISymbol {
        internal FunctionDeclaration(string identifier, FunctionBody functionBody) {
            Identifier = identifier;
            FunctionBody = functionBody;
        }

        public string Identifier { get; private set; }
        public FunctionBody FunctionBody { get; private set; }
        public Dictionary<string, FunctionBody> TypeResolvedFunctionBodies = new Dictionary<string,FunctionBody>();


		#region ISymbol Members

		public string Name {
			get { return Identifier; }
		}

		#endregion
	}

}
