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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting.Generation;
using IronPython.Runtime.Types;

[assembly: PythonModule("operator", typeof(IronPython.Modules.PythonOperator))]
namespace IronPython.Modules {
    public static class PythonOperator {
        public const string __doc__ = "Provides programmatic access to various operators (addition, accessing members, etc...)";

        public class attrgetter {
            private readonly object[] _names;
            public attrgetter(params object[] attrs) {
                if (attrs.Length == 0) throw PythonOps.TypeError("attrgetter expected 1 arguments, got 0");

                this._names = attrs;
            }

            [SpecialName]
            public object Call(CodeContext context, object param) {
                if (_names.Length == 1) {
                    return GetOneAttr(context, param, _names[0]);
                }

                object[] res = new object[_names.Length];
                for (int i = 0; i < _names.Length; i++) {
                    res[i] = GetOneAttr(context, param, _names[i]);
                }
                return PythonTuple.MakeTuple(res);
            }

            private static object GetOneAttr(CodeContext context, object param, object val) {
                string s = val as string;
                if (s == null) {
                    throw PythonOps.TypeError("attribute name must be string");
                }
                int dotPos = s.IndexOf('.');
                if (dotPos >= 0) {
                    object nextParam = GetOneAttr(context, param, s.Substring(0, dotPos));
                    return GetOneAttr(context, nextParam, s.Substring(dotPos + 1, s.Length - dotPos - 1));
                }
                return PythonOps.GetBoundAttr(context, param, SymbolTable.StringToId(s));
            }
        }

        public class itemgetter {
            private readonly object[] _items;

            public itemgetter([NotNull]params object[] items) {
                if (items.Length == 0) {
                    throw PythonOps.TypeError("itemgetter needs at least one argument");
                }
                _items = items;
            }

            [SpecialName]
            public object Call(CodeContext/*!*/ context, object param) {
                if (_items.Length == 1) {
                    return PythonOps.GetIndex(context, param, _items[0]);
                }

                object[] res = new object[_items.Length];
                for (int i = 0; i < _items.Length; i++) {
                    res[i] = PythonOps.GetIndex(context, param, _items[i]);
                }

                return PythonTuple.MakeTuple(res);
            }
        }

        [PythonType]
        public class methodcaller {
            private readonly SymbolId _name;
            private readonly object[] _args;
            private readonly IDictionary<object, object> _dict;

            public methodcaller(string name, params object[] args) {
                _name = SymbolTable.StringToId(name);
                _args = args;
            }

            public methodcaller(string name, [ParamDictionary]IDictionary<object, object> kwargs, params object[] args) {
                _name = SymbolTable.StringToId(name);
                _args = args;
                _dict = kwargs;
            }

            public override string ToString() {
                return String.Format("<operator.methodcaller: {0}>", _name);
            }

            [SpecialName]
            public object Call(CodeContext/*!*/ context, object param) {
                object method = PythonOps.GetBoundAttr(context, param, _name);
                if (_dict == null) {
                    return PythonOps.CallWithContext(context, method, _args);
                } else {
                    return PythonCalls.CallWithKeywordArgs(context, method, _args, _dict);
                }
            }
        }
        
        public static object lt(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.LessThan, a, b);
        }

        public static object le(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.LessThanOrEqual, a, b);
        }

        public static object eq(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Equal, a, b);
        }

        public static object ne(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.NotEqual, a, b);
        }

        public static object ge(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.GreaterThanOrEqual, a, b);
        }

        public static object gt(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.GreaterThan, a, b);
        }

        public static object __lt__(CodeContext/*!*/ context, object a, object b) {
            return lt(context, a, b);
        }

        public static object __le__(CodeContext/*!*/ context, object a, object b) {
            return le(context, a, b);
        }

        public static object __eq__(CodeContext/*!*/ context, object a, object b) {
            return eq(context, a, b);
        }

        public static object __ne__(CodeContext/*!*/ context, object a, object b) {
            return ne(context, a, b);
        }

        public static object __ge__(CodeContext/*!*/ context, object a, object b) {
            return ge(context, a, b);
        }

        public static object __gt__(CodeContext/*!*/ context, object a, object b) {
            return gt(context, a, b);
        }

        public static bool not_(object o) {
            return PythonOps.Not(o);
        }

        public static bool __not__(object o) {
            return PythonOps.Not(o);
        }

        public static bool truth(object o) {
            return PythonOps.IsTrue(o);
        }

        public static object is_(object a, object b) {
            return PythonOps.Is(a, b);
        }

        public static object is_not(object a, object b) {
            return PythonOps.IsNot(a, b);
        }

        public static object abs(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }

        public static object __abs__(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }

        public static object add(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Add, a, b);
        }

        public static object __add__(CodeContext/*!*/ context, object a, object b) {
            return add(context, a, b);
        }

        public static object and_(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.BitwiseAnd, a, b);
        }

        public static object __and__(CodeContext/*!*/ context, object a, object b) {
            return and_(context, a, b);
        }

        public static object div(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Divide, a, b);
        }

        public static object __div__(CodeContext/*!*/ context, object a, object b) {
            return div(context, a, b);
        }

        public static object floordiv(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.FloorDivide, a, b);
        }

        public static object __floordiv__(CodeContext/*!*/ context, object a, object b) {
            return floordiv(context, a, b);
        }

        public static object inv(CodeContext/*!*/ context, object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object invert(CodeContext/*!*/ context, object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object __inv__(CodeContext/*!*/ context, object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object __invert__(CodeContext/*!*/ context, object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object lshift(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.LeftShift, a, b);
        }

        public static object __lshift__(CodeContext/*!*/ context, object a, object b) {
            return lshift(context, a, b);
        }

        public static object mod(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Mod, a, b);
        }

        public static object __mod__(CodeContext/*!*/ context, object a, object b) {
            return mod(context, a, b);
        }

        public static object mul(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Multiply, a, b);
        }

        public static object __mul__(CodeContext/*!*/ context, object a, object b) {
            return mul(context, a, b);
        }

        public static object neg(object o) {
            return PythonOps.Negate(o);
        }

        public static object __neg__(object o) {
            return PythonOps.Negate(o);
        }

        public static object or_(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.BitwiseOr, a, b);
        }

        public static object __or__(CodeContext/*!*/ context, object a, object b) {
            return or_(context, a, b);
        }

        public static object pos(object o) {
            return PythonOps.Plus(o);
        }

        public static object __pos__(object o) {
            return PythonOps.Plus(o);
        }

        public static object pow(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Power, a, b);
        }

        public static object __pow__(CodeContext/*!*/ context, object a, object b) {
            return pow(context, a, b);
        }

        public static object rshift(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.RightShift, a, b);
        }

        public static object __rshift__(CodeContext/*!*/ context, object a, object b) {
            return rshift(context, a, b);
        }

        public static object sub(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.Subtract, a, b);
        }

        public static object __sub__(CodeContext/*!*/ context, object a, object b) {
            return sub(context, a, b);
        }

        public static object truediv(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.TrueDivide, a, b);
        }

        public static object __truediv__(CodeContext/*!*/ context, object a, object b) {
            return truediv(context, a, b);
        }

        public static object xor(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.ExclusiveOr, a, b);
        }

        public static object __xor__(CodeContext/*!*/ context, object a, object b) {
            return xor(context, a, b);
        }

        public static object concat(CodeContext/*!*/ context, object a, object b) {
            TestBothSequence(a, b);

            return PythonContext.GetContext(context).Operation(PythonOperationKind.Add, a, b);
        }

        public static object __concat__(CodeContext/*!*/ context, object a, object b) {
            return concat(context, a, b);
        }

        public static bool contains(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Contains(b, a);
        }

        public static bool __contains__(CodeContext/*!*/ context, object a, object b) {
            return contains(context, a, b);
        }

        public static int countOf(CodeContext/*!*/ context, object a, object b) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(a);
            int count = 0;
            while (e.MoveNext()) {
                if (PythonOps.EqualRetBool(context, e.Current, b)) {
                    count++;
                }
            }
            return count;
        }

        public static void delitem(CodeContext/*!*/ context, object a, object b) {
            PythonContext.GetContext(context).DelIndex(a, b);
        }

        public static void __delitem__(CodeContext/*!*/ context, object a, object b) {
            delitem(context, a, b);
        }

        public static void delslice(CodeContext/*!*/ context, object a, object b, object c) {
            MakeSlice(b, c);

            PythonContext.GetContext(context).DelSlice(a, b, c);            
        }

        public static void __delslice__(CodeContext/*!*/ context, object a, object b, object c) {
            delslice(context, a, b, c);
        }

        public static object getitem(CodeContext/*!*/ context, object a, object b) {
            return PythonOps.GetIndex(context, a, b);
        }

        public static object __getitem__(CodeContext/*!*/ context, object a, object b) {
            return PythonOps.GetIndex(context, a, b);
        }

        public static object getslice(CodeContext/*!*/ context, object a, object b, object c) {
            return PythonOps.GetIndex(context, a, MakeSlice(b, c));
        }

        public static object __getslice__(CodeContext/*!*/ context, object a, object b, object c) {
            return PythonOps.GetIndex(context, a, MakeSlice(b, c));
        }

        public static int indexOf(CodeContext/*!*/ context, object a, object b) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(a);
            int index = 0;
            while (e.MoveNext()) {
                if (PythonOps.EqualRetBool(context, e.Current, b)) {
                    return index;
                }
                index++;
            }
            throw PythonOps.ValueError("object not in sequence");
        }

        public static object repeat(CodeContext context, object a, object b) {
            try {
                PythonOps.GetEnumerator(a);
            } catch {
                throw PythonOps.TypeError("object can't be repeated");
            }
            try {
                Int32Ops.__new__(context, b);
            } catch {
                throw PythonOps.TypeError("integer required");
            }

            return PythonContext.GetContext(context).Operation(PythonOperationKind.Multiply, a, b);
        }

        public static object __repeat__(CodeContext/*!*/  context, object a, object b) {
            return repeat(context, a, b);
        }

        public static object sequenceIncludes(CodeContext/*!*/ context, object a, object b) {
            return contains(context, a, b);
        }

        public static void setitem(CodeContext/*!*/ context, object a, object b, object c) {
            PythonContext.GetContext(context).SetIndex(a, b, c);
        }

        public static void __setitem__(CodeContext/*!*/ context, object a, object b, object c) {
            setitem(context, a, b, c);
        }

        public static void setslice(CodeContext/*!*/ context, object a, object b, object c, object v) {
            PythonContext.GetContext(context).SetSlice(a, b, c, v);
        }

        public static void __setslice__(CodeContext/*!*/ context, object a, object b, object c, object v) {
            setslice(context, a, b, c, v);
        }

        public static bool isCallable(CodeContext/*!*/ context, object o) {
            return PythonOps.IsCallable(context, o);
        }

        public static object isMappingType(CodeContext context, object o) {
            return PythonOps.IsMappingType(context, o);
        }

        public static bool isNumberType(object o) {
            return o is int ||
                o is long ||
                o is double ||
                o is float ||
                o is short ||
                o is uint ||
                o is ulong ||
                o is ushort ||
                o is decimal ||
                o is BigInteger ||
                o is Complex64 ||
                o is byte;
        }

        public static bool isSequenceType(object o) {
            return
                   CompilerHelpers.GetType(o) != typeof(PythonType) &&
                   !(o is PythonDictionary) && (
                   o is System.Collections.ICollection ||
                   o is System.Collections.IEnumerable ||
                   o is System.Collections.IEnumerator ||
                   o is System.Collections.IList ||
                   PythonOps.HasAttr(DefaultContext.Default, o, Symbols.GetItem));
        }

        private static int SliceToInt(object o) {
            int i;
            if (Converter.TryConvertToInt32(o, out i)) return i;
            throw PythonOps.TypeError("integer expected");
        }

        private static object MakeSlice(object a, object b) {
            return new Slice(SliceToInt(a), SliceToInt(b), null);
        }
        
        public static object iadd(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceAdd, a, b);
        }

        public static object iand(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceBitwiseAnd, a, b);
        }

        public static object idiv(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceDivide, a, b);
        }

        public static object ifloordiv(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceFloorDivide, a, b);
        }

        public static object ilshift(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceLeftShift, a, b);
        }

        public static object imod(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceMod, a, b);
        }

        public static object imul(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceMultiply, a, b);
        }

        public static object ior(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceBitwiseOr, a, b);
        }

        public static object ipow(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlacePower, a, b);
        }

        public static object irshift(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceRightShift, a, b);
        }

        public static object isub(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceSubtract, a, b);
        }

        public static object itruediv(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceTrueDivide, a, b);
        }

        public static object ixor(CodeContext/*!*/ context, object a, object b) {
            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceExclusiveOr, a, b);
        }

        public static object iconcat(CodeContext/*!*/ context, object a, object b) {
            TestBothSequence(a, b);

            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceAdd, a, b);
        }

        public static object irepeat(CodeContext/*!*/ context, object a, object b) {
            if (!isSequenceType(a)) {
                throw PythonOps.TypeError("'{0}' object cannot be repeated", PythonTypeOps.GetName(a));
            }

            try {
                Int32Ops.__new__(DefaultContext.Default, b);
            } catch {
                throw PythonOps.TypeError("integer required");
            }

            return PythonContext.GetContext(context).Operation(PythonOperationKind.InPlaceMultiply, a, b);
        }

        public static object __iadd__(CodeContext/*!*/ context, object a, object b) {
            return iadd(context, a, b);
        }

        public static object __iand__(CodeContext/*!*/ context, object a, object b) {
            return iand(context, a, b);
        }

        public static object __idiv__(CodeContext/*!*/ context, object a, object b) {
            return idiv(context, a, b);
        }

        public static object __ifloordiv__(CodeContext/*!*/ context, object a, object b) {
            return ifloordiv(context, a, b);
        }

        public static object __ilshift__(CodeContext/*!*/ context, object a, object b) {
            return ilshift(context, a, b);
        }

        public static object __imod__(CodeContext/*!*/ context, object a, object b) {
            return imod(context, a, b);
        }

        public static object __imul__(CodeContext/*!*/ context, object a, object b) {
            return imul(context, a, b);
        }

        public static object __ior__(CodeContext/*!*/ context, object a, object b) {
            return ior(context, a, b);
        }

        public static object __ipow__(CodeContext/*!*/ context, object a, object b) {
            return ipow(context, a, b);
        }

        public static object __irshift__(CodeContext/*!*/ context, object a, object b) {
            return irshift(context, a, b);
        }

        public static object __isub__(CodeContext/*!*/ context, object a, object b) {
            return isub(context, a, b);
        }

        public static object __itruediv__(CodeContext/*!*/ context, object a, object b) {
            return itruediv(context, a, b);
        }

        public static object __ixor__(CodeContext/*!*/ context, object a, object b) {
            return ixor(context, a, b);
        }

        public static object __iconcat__(CodeContext/*!*/ context, object a, object b) {
            return iconcat(context, a, b);
        }

        public static object __irepeat__(CodeContext/*!*/ context, object a, object b) {
            return irepeat(context, a, b);
        }

        public static object index(object a) {
            return __index__(a);
        }

        public static int __index__(object a) {
            return Converter.ConvertToIndex(a);
        }

        private static void TestBothSequence(object a, object b) {
            if (!isSequenceType(a)) {
                throw PythonOps.TypeError("'{0}' object cannot be concatenated", PythonTypeOps.GetName(a));
            } else if (!isSequenceType(b)) {
                throw PythonOps.TypeError("cannot concatenate '{0}' and '{1} objects", PythonTypeOps.GetName(a), PythonTypeOps.GetName(b));
            }
        }
    }
}
