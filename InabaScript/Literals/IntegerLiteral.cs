using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {
    public class IntegerLiteral : IExpression {

        internal IntegerLiteral(string number) {
            Number = long.Parse(number);
        }

        public long Number { get; private set; }

    }

}
