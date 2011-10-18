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

        internal VariableDeclaration(string identifier, IType type)
        {
            Identifier = identifier;
            Type = type;
		}

        public string Identifier { get; private set; }
        public IExpression Initializer { get; private set; }

		#region ISymbol Members

		public string Name {
			get { return Identifier; }
		}


		#endregion


        public IType Type
        {
            get;
            private set;
        }
    }
}
