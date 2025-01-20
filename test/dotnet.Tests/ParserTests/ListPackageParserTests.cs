// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests.ParserTests
{
    public class ListPackageParserTests
    {
        [TestMethod]
        public void ListPackageCanForwardInteractiveFlag()
        {
            var result = Parser.Instance.Parse("dotnet list package --interactive");

            result.OptionValuesToBeForwarded(ListPackageReferencesCommandParser.GetCommand()).Should().ContainSingle("--interactive");
            result.Errors.Should().BeEmpty();
        }

        [TestMethod]
        [DataRow("--verbosity", "foo")]
        [DataRow("--verbosity", "")]
        [DataRow("-v", "foo")]
        [DataRow("-v", "")]
        public void ListPackageRejectsInvalidVerbosityFlags(string inputOption, string value)
        {
            var result = Parser.Instance.Parse($"dotnet list package {inputOption} {value}");

            result.Errors.Should().NotBeEmpty();
        }

        [TestMethod]
        [DataRow("--verbosity", "q")]
        [DataRow("--verbosity", "quiet")]
        [DataRow("--verbosity", "m")]
        [DataRow("--verbosity", "minimal")]
        [DataRow("--verbosity", "n")]
        [DataRow("--verbosity", "normal")]
        [DataRow("--verbosity", "d")]
        [DataRow("--verbosity", "detailed")]
        [DataRow("--verbosity", "diag")]
        [DataRow("--verbosity", "diagnostic")]
        [DataRow("--verbosity", "QUIET")]
        [DataRow("-v", "q")]
        [DataRow("-v", "QUIET")]
        public void ListPackageCanForwardVerbosityFlag(string inputOption, string value)
        {
            var result = Parser.Instance.Parse($"dotnet list package {inputOption} {value}");

            result
                .OptionValuesToBeForwarded(ListPackageReferencesCommandParser.GetCommand())
                .Should()
                .Contain($"--verbosity:{value.ToLowerInvariant()}");
            result.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void ListPackageDoesNotForwardVerbosityByDefault()
        {
            var result = Parser.Instance.Parse($"dotnet list package");

            result
                .OptionValuesToBeForwarded(ListPackageReferencesCommandParser.GetCommand())
                .Should()
                .NotContain(i => i.Contains("--verbosity", StringComparison.OrdinalIgnoreCase))
                .And.NotContain(i => i.Contains("-v", StringComparison.OrdinalIgnoreCase));
            result.Errors.Should().BeEmpty();
        }
    }
}
