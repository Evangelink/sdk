// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.ParserTests
{
    public class ToolRunParserTests
    {
        private readonly MSTestContext testContext;

        public ToolRunParserTests(MSTestContext testContext)
        {
            this.testContext = testContext;
        }

        [TestMethod]
        public void ListToolParserCanGetToolCommandNameArgument()
        {
            var result = Parser.Instance.Parse("dotnet tool run dotnetsay");

            var packageId = result.GetValue<string>(ToolRunCommandParser.CommandNameArgument);

            packageId.Should().Be("dotnetsay");
        }

        [TestMethod]
        public void ListToolParserCanGetCommandsArgumentInUnmatchedTokens()
        {
            var result = Parser.Instance.Parse("dotnet tool run dotnetsay hi");

            result.ShowHelpOrErrorIfAppropriate(); // Should not throw error
        }

        [TestMethod]
        public void ListToolParserCanGetCommandsArgumentInUnparsedTokens()
        {
            var result = Parser.Instance.Parse("dotnet tool run dotnetsay -- hi");

            result.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void ListToolParserCanGetCommandsArgumentInUnparsedTokens2()
        {
            var result = Parser.Instance.Parse("dotnet tool run dotnetsay hi1 -- hi2");

            result.ShowHelpOrErrorIfAppropriate(); // Should not throw error
        }

        [TestMethod]
        public void RootSubCommandIsToolCommand()
        {
            var result = Parser.Instance.Parse("dotnetsay run -v arg");
            result.RootSubCommandResult().Should().Be("dotnetsay");
        }
    }
}
