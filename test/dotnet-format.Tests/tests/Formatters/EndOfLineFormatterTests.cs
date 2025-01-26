// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Tools.Formatters;

namespace Microsoft.CodeAnalysis.Tools.Tests.Formatters
{
    [TestClass]
    public class EndOfLineFormatterTests : CSharpFormatterTests
    {
        private protected override ICodeFormatter Formatter => new EndOfLineFormatter();

        public EndOfLineFormatterTests(MSTestContext testContext)
        {
            TestContext = testContext;
        }

        [TestMethod]
        [DataRow("\n", "\n", "lf")]
        [DataRow("\r\n", "\n", "lf")]
        [DataRow("\r", "\n", "lf")]
        [DataRow("\n", "\r\n", "crlf")]
        [DataRow("\r\n", "\r\n", "crlf")]
        [DataRow("\r", "\r\n", "crlf")]
        [DataRow("\n", "\r", "cr")]
        [DataRow("\r\n", "\r", "cr")]
        [DataRow("\r", "\r", "cr")]
        public async Task TestEndOfLine_NoFinalNewline(string codeNewline, string expectedNewline, string endOfLine)
        {
            var testCode = $"class C{codeNewline}{{{codeNewline}}}";

            var expectedCode = $"class C{expectedNewline}{{{expectedNewline}}}";

            var editorConfig = new Dictionary<string, string>()
            {
                ["end_of_line"] = endOfLine,
            };

            await AssertCodeChangedAsync(testCode, expectedCode, editorConfig);
        }

        [TestMethod]
        [DataRow("\n", "\n", "lf")]
        [DataRow("\r\n", "\n", "lf")]
        [DataRow("\r", "\n", "lf")]
        [DataRow("\n", "\r\n", "crlf")]
        [DataRow("\r\n", "\r\n", "crlf")]
        [DataRow("\r", "\r\n", "crlf")]
        [DataRow("\n", "\r", "cr")]
        [DataRow("\r\n", "\r", "cr")]
        [DataRow("\r", "\r", "cr")]
        public async Task TestEndOfLine_WithFinalNewline(string codeNewline, string expectedNewline, string endOfLine)
        {
            var testCode = $"class C{codeNewline}{{{codeNewline}}}{codeNewline}";

            var expectedCode = $"class C{expectedNewline}{{{expectedNewline}}}{expectedNewline}";

            var editorConfig = new Dictionary<string, string>()
            {
                ["end_of_line"] = endOfLine,
            };

            await AssertCodeChangedAsync(testCode, expectedCode, editorConfig);
        }

        [TestMethod]
        [DataRow("\n")]
        [DataRow("\r\n")]
        [DataRow("\r")]
        public async Task TestEndOfLine_AndNoSetting_NoChanges(string codeNewline)
        {
            var testCode = $"class C{codeNewline}{{{codeNewline}}}{codeNewline}";

            var editorConfig = new Dictionary<string, string>();

            await AssertCodeUnchangedAsync(testCode, editorConfig);
        }
    }
}
