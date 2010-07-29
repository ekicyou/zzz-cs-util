using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace CSUtil.CodeDom.ExpressionBuilder
{
    internal class BuilderPrimitive : ET
    {
        private readonly object Value;
        public BuilderPrimitive(object value)
        {
            this.Value = value;
        }

        public override CodeExpression Expression
        {
            get { return new CodePrimitiveExpression(Value); }
        }

    }
}
