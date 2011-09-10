using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {


	public class VariableDeclaration : IStatement, ISymbol {
        internal VariableDeclaration(string identifier, IExpression initializer) {
            Identifier = identifier;
            Initializer = initializer;
		}

        internal VariableDeclaration(string identifier, string type)
        {
            Identifier = identifier;
		}

        public string Identifier { get; private set; }
        public IExpression Initializer { get; private set; }

		#region ISymbol Members

		public string Name {
			get { return Identifier; }
		}


		#endregion
	}
}
