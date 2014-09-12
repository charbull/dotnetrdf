/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:cube() function
    /// </summary>
    public class CubeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Cube Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public CubeFunction(IExpression expr)
            : base(expr) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode temp = this._expr.Evaluate(solution, context);
            if (temp == null) throw new RdfQueryException("Cannot square a null");

            switch (temp.NumericType)
            {
                case EffectiveNumericType.Integer:
                    long l = temp.AsInteger();
                    return new LongNode(l * l * l);
                case EffectiveNumericType.Decimal:
                    decimal d = temp.AsDecimal();
                    return new DecimalNode(d * d * d);
                case EffectiveNumericType.Float:
                    float f = temp.AsFloat();
                    return new FloatNode(f * f * f);
                case EffectiveNumericType.Double:
                    double dbl = temp.AsDouble();
                    return new DoubleNode(Math.Pow(dbl, 3));
                case EffectiveNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of this expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override IExpression Transform(IExpressionTransformer transformer)
        {
            return new CubeFunction(transformer.Transform(this._expr));
        }
    }
}