using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InabaScript
{

    public class FunctionCall : IStatement, IExpression
    {
        internal FunctionCall(IExpression funcReturner, params IExpression[] callingExpressions)
            : this(funcReturner, new List<IExpression>(callingExpressions))
        {
        }

        internal FunctionCall(IExpression funcReturner, List<IExpression> callingExpressions)
        {
            CallingExpressions = callingExpressions;
            Type = (funcReturner.Type as FunctionType).ReturnType;
        }

        #region IExpression Members

        public List<IExpression> CallingExpressions { get; private set; }

        public FunctionDeclaration TheCalledOne { get; internal set; }  // Gets set by scope and type resolver

        #endregion

        public IType Type
        {
            get;
            private set;
        }
    }
}
