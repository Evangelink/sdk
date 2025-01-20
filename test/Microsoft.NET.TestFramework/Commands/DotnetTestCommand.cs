// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework.Commands
{
    public class DotnetTestCommand : DotnetCommand
    {
        public DotnetTestCommand(MSTestContext testContext, bool disableNewOutput, params string[] args) : base(testContext)
        {
            Arguments.Add("test");
            if (disableNewOutput)
            {
                Arguments.Add("--property:VsTestUseMSBuildOutput=false");
                Arguments.Add("-tl:false");
            }

            Arguments.AddRange(args);
        }
    }
}
