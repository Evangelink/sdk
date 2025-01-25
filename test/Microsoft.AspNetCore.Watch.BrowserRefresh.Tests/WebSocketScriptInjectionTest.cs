// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Watch.BrowserRefresh
{
    public class WebSocketScriptInjectionTest
    {
        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_DoesNotInjectMarkup_IfInputDoesNotContainBodyTag()
        {
            // Arrange
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("<div>this is not a real body tag.</div>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(input, stream.ToArray());
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_InjectsMarkupIfBodyTagAppearsInTheMiddle()
        {
            // Arrange
            var expected =
$@"<footer>
    This is the footer
</footer>
{WebSocketScriptInjection.InjectedScript}</body>
</html>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes(
@"<footer>
    This is the footer
</footer>
</body>
</html>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output, ignoreLineEndingDifferences: true);
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_WithOffsetBodyTagAppearsInMiddle()
        {
            // Arrange
            var expected = $"</table>{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("unused</table></body>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input.AsMemory(6));

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_WithOffsetBodyTagAppearsAtStartOfOffset()
        {
            // Arrange
            var expected = $"{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("unused</body>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input.AsMemory(6));

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_InjectsMarkupIfBodyTagAppearsAtTheStartOfOutput()
        {
            // Arrange
            var expected = $"{WebSocketScriptInjection.InjectedScript}</body></html>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("</body></html>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_InjectsMarkupIfBodyTagAppearsByItself()
        {
            // Arrange
            var expected = $"{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("</body>");

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public async Task TryInjectLiveReloadScriptAsync_MultipleBodyTags()
        {
            // Arrange
            var expected = $"<p></body>some text</p>{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("abc<p></body>some text</p></body>").AsMemory(3);

            // Act
            var result = await WebSocketScriptInjection.TryInjectLiveReloadScriptAsync(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TryInjectLiveReloadScript_NoBodyTag()
        {
            // Arrange
            var expected = "<p>Hello world</p>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes(expected).AsSpan();

            // Act
            var result = WebSocketScriptInjection.TryInjectLiveReloadScript(stream, input);

            // Assert
            Assert.IsFalse(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TryInjectLiveReloadScript_NoOffset()
        {
            // Arrange
            var expected = $"</table>{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("</table></body>").AsSpan();

            // Act
            var result = WebSocketScriptInjection.TryInjectLiveReloadScript(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TryInjectLiveReloadScript_WithOffset()
        {
            // Arrange
            var expected = $"</table>{WebSocketScriptInjection.InjectedScript}</body>";
            var stream = new MemoryStream();
            var input = Encoding.UTF8.GetBytes("unused</table></body>").AsSpan(6);

            // Act
            var result = WebSocketScriptInjection.TryInjectLiveReloadScript(stream, input);

            // Assert
            Assert.IsTrue(result);
            var output = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual(expected, output);
        }
    }
}
