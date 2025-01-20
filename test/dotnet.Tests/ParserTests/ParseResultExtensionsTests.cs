// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.ParserTests
{
    public class ParseResultExtensionsTests
    {
        private readonly MSTestContext testContext;

        public ParseResultExtensionsTests(MSTestContext testContext)
        {
            this.testContext = testContext;
        }

        [TestMethod]
        [DataRow("build /p:prop=true", "build")]
        [DataRow("add package", "add")]
        [DataRow("watch run", "watch")]
        [DataRow("watch run -h", "watch")]
        [DataRow("ignore list", "ignore")] // Global tool
        public void RootSubCommandResultReturnsCorrectSubCommand(string input, string expected)
        {
            var result = Parser.Instance.Parse(input);

            result.RootSubCommandResult()
                .Should()
                .Be(expected);
        }

        [TestMethod]
        [DataRow(new string[] { "dotnet", "build" }, new string[] { })]
        [DataRow(new string[] { "build" }, new string[] { })]
        [DataRow(new string[] { "dotnet", "test", "-d" }, new string[] { "-d" })]
        [DataRow(new string[] { "dotnet", "publish", "-o", "foo" }, new string[] { "-o", "foo" })]
        [DataRow(new string[] { "publish", "-o", "foo" }, new string[] { "-o", "foo" })]
        [DataRow(new string[] { "dotnet", "add", "package", "-h" }, new string[] { "package", "-h" })]
        [DataRow(new string[] { "add", "package", "-h" }, new string[] { "package", "-h" })]
        [DataRow(new string[] { "dotnet", "-d", "help" }, new string[] { })]
        [DataRow(new string[] { "dotnet", "run", "--", "-d" }, new string[] { "--", "-d" })]
        public void GetSubArgumentsRemovesTopLevelCommands(string[] input, string[] expected)
        {
            input.GetSubArguments()
                .Should().BeEquivalentTo(expected);
        }
    }
}
