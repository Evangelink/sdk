// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.NET.TestFramework.Commands
{
    public class AddReferenceCommand : DotnetCommand
    {
        private string? _projectName = null;

        public AddReferenceCommand(MSTestContext testContext, params string[] args) : base(testContext, args)
        {
        }

        public override CommandResult Execute(IEnumerable<string> args)
        {
            List<string> newArgs = new();
            newArgs.Add("add");
            if (!string.IsNullOrEmpty(_projectName))
            {
                newArgs.Add(_projectName);
            }
            newArgs.Add("reference");
            newArgs.AddRange(args);

            return base.Execute(newArgs);
        }


        public AddReferenceCommand WithProject(string projectName)
        {
            _projectName = projectName;
            return this;
        }
    }
}
