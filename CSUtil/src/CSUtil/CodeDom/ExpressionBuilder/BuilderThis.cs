using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace CSUtil.CodeDom.ExpressionBuilder
{
    internal class BuilderThis : ET
    {
        public override CodeExpression Expression { get { return new CodeThisReferenceExpression(); } }
    }
}
