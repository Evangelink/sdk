// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLineValidation;
using Microsoft.DotNet.Tools.Common;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.ParserTests
{
    public class AddReferenceParserTests
    {
        private readonly MSTestContext testContext;

        public AddReferenceParserTests(MSTestContext testContext)
        {
            this.testContext = testContext;
        }

        [TestMethod]
        public void AddReferenceHasDefaultArgumentSetToCurrentDirectory()
        {
            var result = Parser.Instance.Parse("dotnet add reference my.csproj");

            result.GetValue<string>(AddCommandParser.ProjectArgument)
                .Should()
                .BeEquivalentTo(
                    PathUtility.EnsureTrailingSlash(Directory.GetCurrentDirectory()));
        }

        [TestMethod]
        public void AddReferenceHasInteractiveFlag()
        {
            var result = Parser.Instance.Parse("dotnet add reference my.csproj --interactive");

            result.GetValue<bool>(AddProjectToProjectReferenceParser.InteractiveOption)
                .Should().BeTrue();
        }

        [TestMethod]
        public void AddReferenceDoesNotHaveInteractiveFlagByDefault()
        {
            var result = Parser.Instance.Parse("dotnet add reference my.csproj");

            result.GetValue<bool>(AddProjectToProjectReferenceParser.InteractiveOption)
                .Should().BeFalse();
        }

        [TestMethod]
        public void AddReferenceWithoutArgumentResultsInAnError()
        {
            var result = Parser.Instance.Parse("dotnet add reference");

            result
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(string.Format(LocalizableStrings.RequiredArgumentMissingForCommand, "'reference'."));
        }
    }
}
