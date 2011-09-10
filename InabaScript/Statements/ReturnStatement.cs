using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

    public class ReturnStatement : IStatement {
        internal ReturnStatement(IExpression expr) {
            ReturnedExpr = expr;
        }

        public IExpression ReturnedExpr { get; private set; }
    }

}
