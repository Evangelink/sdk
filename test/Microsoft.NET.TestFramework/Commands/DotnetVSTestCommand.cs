// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework.Commands
{
    public class DotnetVSTestCommand : DotnetCommand
    {
        public DotnetVSTestCommand(MSTestContext testContext, params string[] args) : base(testContext)
        {
            Arguments.Add("vstest");
            Arguments.AddRange(args);
        }
    }
}
