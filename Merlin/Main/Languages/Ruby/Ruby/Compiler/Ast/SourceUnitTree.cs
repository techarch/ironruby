/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using MSA = System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public partial class SourceUnitTree : Node {

        private readonly LexicalScope/*!*/ _definedScope;
        private readonly List<Initializer> _initializers;
        private readonly Statements/*!*/ _statements;
        private readonly RubyEncoding/*!*/ _encoding;

        // An offset of the first byte after __END__ that can be read via DATA constant or -1 if __END__ is not present.
        private readonly int _dataOffset;

        public List<Initializer> Initializers {
            get { return _initializers; }
        }

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public SourceUnitTree(LexicalScope/*!*/ definedScope, Statements/*!*/ statements, List<Initializer> initializers, 
            RubyEncoding/*!*/ encoding, int dataOffset)
            : base(SourceSpan.None) {
            Assert.NotNull(definedScope, statements, encoding);

            _definedScope = definedScope;
            _statements = statements;
            _initializers = initializers;
            _encoding = encoding;
            _dataOffset = dataOffset;
        }

        private ScopeBuilder/*!*/ DefineLocals() {
            return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), null, _definedScope);
        }

        internal MSA.Expression<T>/*!*/ Transform<T>(AstGenerator/*!*/ gen) {
            Debug.Assert(gen != null);

            ScopeBuilder scope = DefineLocals();

            MSA.ParameterExpression[] parameters;
            MSA.ParameterExpression selfVariable;
            MSA.ParameterExpression runtimeScopeVariable;
            MSA.ParameterExpression blockParameter;

            if (gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.None ||
                gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.ModuleEval) {
                parameters = new MSA.ParameterExpression[4];

                runtimeScopeVariable = parameters[0] = Ast.Parameter(typeof(RubyScope), "#scope");
                selfVariable = parameters[1] = Ast.Parameter(typeof(object), "#self");
                parameters[2] = Ast.Parameter(typeof(RubyModule), "#module");
                blockParameter = parameters[3] = Ast.Parameter(typeof(Proc), "#block");
            } else {
                parameters = new MSA.ParameterExpression[2];

                runtimeScopeVariable = parameters[0] = Ast.Parameter(typeof(RubyScope), "#scope");
                selfVariable = parameters[1] = Ast.Parameter(typeof(object), "#self");

                blockParameter = null;
            }

            gen.EnterSourceUnit(
                scope,
                selfVariable,
                runtimeScopeVariable,
                blockParameter,
                gen.CompilerOptions.TopLevelMethodName, // method name
                null                                    // parameters
            );

            MSA.Expression body;


            if (_statements.Count > 0) {
                if (gen.PrintInteractiveResult) {
                    var resultVariable = scope.DefineHiddenVariable("#result", typeof(object));

                    var epilogue = Methods.PrintInteractiveResult.OpCall(runtimeScopeVariable,
                        Ast.Dynamic(ConvertToSAction.Make(gen.Context), typeof(MutableString),
                            CallBuilder.InvokeMethod(gen.Context, "inspect", RubyCallSignature.WithScope(0),
                                gen.CurrentScopeVariable, resultVariable
                            )
                        )
                    );

                    body = gen.TransformStatements(null, _statements, epilogue, ResultOperation.Store(resultVariable));
                } else {
                    body = gen.TransformStatements(_statements, ResultOperation.Return);
                }

                // TODO:
                var exceptionVariable = Ast.Parameter(typeof(Exception), "#exception");
                body = AstUtils.Try(
                    body
                ).Filter(exceptionVariable, Methods.TraceTopLevelCodeFrame.OpCall(runtimeScopeVariable, exceptionVariable),
                    Ast.Empty()
                ).Finally(
                    LeaveInterpretedFrameExpression.Instance
                );
            } else {
                body = AstUtils.Constant(null);
            }

            // scope initialization:
            MSA.Expression prologue;
            switch (gen.CompilerOptions.FactoryKind) {
                case TopScopeFactoryKind.None:
                case TopScopeFactoryKind.ModuleEval:
                    prologue = Methods.InitializeScopeNoLocals.OpCall(runtimeScopeVariable, EnterInterpretedFrameExpression.Instance);
                    break;

                case TopScopeFactoryKind.Hosted:
                case TopScopeFactoryKind.File:
                case TopScopeFactoryKind.WrappedFile:
                    prologue = Methods.InitializeScope.OpCall(
                        runtimeScopeVariable, scope.MakeLocalsStorage(), scope.GetVariableNamesExpression(),
                        EnterInterpretedFrameExpression.Instance
                    );
                    break;

                case TopScopeFactoryKind.Main:
                    prologue = Methods.InitializeScope.OpCall(
                        runtimeScopeVariable, scope.MakeLocalsStorage(), scope.GetVariableNamesExpression(),
                        EnterInterpretedFrameExpression.Instance
                    );
                    if (_dataOffset >= 0) {
                        prologue = Ast.Block(
                            prologue,
                            Methods.SetDataConstant.OpCall(
                                runtimeScopeVariable,
                                gen.SourcePathConstant,
                                AstUtils.Constant(_dataOffset)
                            )
                        );
                    }
                    break;

                default:
                    throw Assert.Unreachable;
            }

            body = gen.AddReturnTarget(scope.CreateScope(Ast.Block(prologue, body)));

            gen.LeaveSourceUnit();

            return Ast.Lambda<T>(body, GetEncodedName(gen), parameters);
        }

        private static string/*!*/ GetEncodedName(AstGenerator/*!*/ gen) {
            return RubyExceptionData.EncodeMethodName(RubyExceptionData.TopLevelMethodName, gen.SourcePath, SourceSpan.None);
        }
    }
}
