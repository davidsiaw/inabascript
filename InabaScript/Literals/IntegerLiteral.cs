using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class IntegerLiteral : IExpression {

        internal IntegerLiteral(string number) {
            Number = long.Parse(number);
            Type = new IntegerRangeType();
        }

        public long Number { get; private set; }


        public IType Type
        {
            get;
            private set;
        }
    }

}
