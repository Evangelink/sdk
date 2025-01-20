// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework.Commands
{
    public class DotnetRestoreCommand : DotnetCommand
    {
        public DotnetRestoreCommand(MSTestContext testContext, params string[] args) : base(testContext)
        {
            Arguments.Add("restore");
            Arguments.AddRange(args);
        }
    }
}
