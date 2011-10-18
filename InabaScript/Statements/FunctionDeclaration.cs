using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

	public class FunctionDeclaration : IStatement, IExpression, ISymbol {
        internal FunctionDeclaration(string identifier, FunctionBody functionBody) {
            Identifier = identifier;
            FunctionBody = functionBody;

            List<IType> paramtypes = new List<IType>(functionBody.Parameters.Select(x => x.Type as IType));
            IType returnType = GetReturnType(functionBody);

            Type = new FunctionType(paramtypes, returnType);
        }

        private static IType GetReturnType(FunctionBody functionBody)
        {

            var retstmt = functionBody.Statements.First(x => x is ReturnStatement);
            IType returnType = null;
            if (retstmt != null)
            {
                returnType = (retstmt as ReturnStatement).ReturnedExpr.Type;
            }
            return returnType;
        }

        public string Identifier { get; private set; }
        public FunctionBody FunctionBody { get; private set; }
        public Dictionary<string, FunctionBody> TypeResolvedFunctionBodies = new Dictionary<string,FunctionBody>();



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
