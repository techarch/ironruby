﻿/* ****************************************************************************
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Hosting;
using IronRuby.Runtime;
using System.Security;

[assembly: AssemblyTitle("Ruby Libraries")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Ruby")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("ca75230d-3011-485d-b1db-dfe924b6c434")]

#if !SILVERLIGHT
[assembly: AssemblyVersion(RubyContext.IronRubyVersionString)]
[assembly: AssemblyFileVersion(RubyContext.IronRubyVersionString)]
[assembly: AllowPartiallyTrustedCallers]
#endif

[assembly: SecurityTransparent]
#if CLR4
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif