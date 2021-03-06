/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class AssertStatement : Statement {
        private readonly Expression _test, _message;

        public AssertStatement(Expression test, Expression message) {
            _test = test;
            _message = message;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Message {
            get { return _message; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // If debugging is off, return empty statement
            if (ag.Optimize) {
                return AstUtils.Empty();
            }

            // Transform into:
            // if (_test) {
            // } else {
            //     RaiseAssertionError(_message);
            // }
            return ag.AddDebugInfoAndVoid(
                AstUtils.Unless(                                 // if
                    ag.TransformAndDynamicConvert(_test, typeof(bool)), // _test
                    Ast.Call(                                           // else branch
                        AstGenerator.GetHelperMethod("RaiseAssertionError"),
                        ag.TransformOrConstantNull(_message, typeof(object))
                    )
                ),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_message != null) {
                    _message.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
