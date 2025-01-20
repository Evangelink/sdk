// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework.Commands
{
    public class DotnetPublishCommand : DotnetCommand
    {
        private string? _runtime;

        public DotnetPublishCommand(MSTestContext testContext, params string[] args) : base(testContext)
        {
            Arguments.Add("publish");
            Arguments.AddRange(args);
        }

        protected override SdkCommandSpec CreateCommand(IEnumerable<string> args)
        {
            List<string> newArgs = new(args);
            if (!string.IsNullOrEmpty(_runtime))
            {
                newArgs.Add("-r");
                newArgs.Add(_runtime);
            }

            return base.CreateCommand(newArgs);
        }

        public DotnetPublishCommand WithRuntime(string runtime)
        {
            _runtime = runtime;
            return this;
        }
    }

}
