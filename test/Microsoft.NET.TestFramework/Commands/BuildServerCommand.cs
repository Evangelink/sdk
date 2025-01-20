// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public sealed class BuildServerCommand : DotnetCommand
    {
        public BuildServerCommand(MSTestContext testContext, params string[] args) : base(testContext, args)
        {
        }

        protected override SdkCommandSpec CreateCommand(IEnumerable<string> args)
        {
            List<string> newArgs = new()
            {
                "build-server"
            };
            newArgs.AddRange(args);

            return base.CreateCommand(newArgs);
        }
    }
}
