// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.NET.TestFramework.Commands
{
    public class ListReferenceCommand : DotnetCommand
    {
        private string? _projectName = null;

        public ListReferenceCommand(MSTestContext testContext, params string[] args) : base(testContext, args)
        {
        }

        public override CommandResult Execute(IEnumerable<string> args)
        {
            List<string> newArgs = new();
            newArgs.Add("list");
            if (!string.IsNullOrEmpty(_projectName))
            {
                newArgs.Add(_projectName);
            }
            newArgs.Add("reference");
            newArgs.AddRange(args);

            return base.Execute(newArgs);
        }


        public ListReferenceCommand WithProject(string projectName)
        {
            _projectName = projectName;
            return this;
        }
    }
}
