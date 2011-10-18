using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript {

	public class MemberIdentifier : IExpression {

		public MemberIdentifier(IExpression parentExpression, string ident) {
			this.ParentExpression = parentExpression;
			this.Identifier = ident;
		}

		public string Identifier { get; private set; }
		public IExpression ParentExpression { get; private set; }

	}

    public class InnerReference : IExpression {
        internal InnerReference(IExpression lhs) {
            LHS = lhs;
        }

        internal void SetMemberIdent(string ident) {
			MemberIdent = new MemberIdentifier(LHS, ident);
        }

        public IExpression LHS { get; private set; }
        public IExpression MemberIdent { get; private set; }
    }
}
