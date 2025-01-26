// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Cli.Test.Tests
{
    [TestClass]
    public class TestCommandParserTests
    {
        [TestMethod]
        public void SurroundWithDoubleQuotesWithNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TestCommandParser.SurroundWithDoubleQuotes(null));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("\"a\"")]
        [DataRow("\"aaa\"")]
        public void SurroundWithDoubleQuotesWhenAlreadySurroundedDoesNothing(string input)
        {
            var escapedInput = "\"" + input + "\"";
            var result = TestCommandParser.SurroundWithDoubleQuotes(escapedInput);
            result.Should().Be(escapedInput);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("aaa")]
        [DataRow("\"a")]
        [DataRow("a\"")]
        public void SurroundWithDoubleQuotesWhenNotSurroundedSurrounds(string input)
        {
            var result = TestCommandParser.SurroundWithDoubleQuotes(input);
            result.Should().Be("\"" + input + "\"");
        }

        [TestMethod]
        [DataRow("\\\\")]
        [DataRow("\\\\\\\\")]
        [DataRow("/\\\\")]
        [DataRow("/\\/\\/\\\\")]
        public void SurroundWithDoubleQuotesHandlesCorrectlyEvenCountOfTrailingBackslashes(string input)
        {
            var result = TestCommandParser.SurroundWithDoubleQuotes(input);
            result.Should().Be("\"" + input + "\"");
        }

        [TestMethod]
        [DataRow("\\")]
        [DataRow("\\\\\\")]
        [DataRow("/\\")]
        [DataRow("/\\/\\/\\")]
        public void SurroundWithDoubleQuotesHandlesCorrectlyOddCountOfTrailingBackslashes(string input)
        {
            var result = TestCommandParser.SurroundWithDoubleQuotes(input);
            result.Should().Be("\"" + input + "\\\"");
        }
    }
}
