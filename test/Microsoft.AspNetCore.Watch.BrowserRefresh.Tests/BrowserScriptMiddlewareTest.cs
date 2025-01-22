// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Watch.BrowserRefresh
{
    public class BrowserScriptMiddlewareTest
    {
        private readonly RequestDelegate _next = (context) => Task.CompletedTask;
        private readonly ILogger<BrowserScriptMiddleware> _logger;

        public BrowserScriptMiddlewareTest()
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            _logger = loggerFactory.CreateLogger<BrowserScriptMiddleware>();
        }

        [TestMethod]
        public async Task InvokeAsync_ReturnsScript()
        {
            var context = new DefaultHttpContext();
            var stream = new MemoryStream();
            context.Response.Body = stream;
            var middleware = new BrowserScriptMiddleware(
                _next,
                new PathString("/script.js"),
                BrowserScriptMiddleware.GetWebSocketClientJavaScript("some-host", "test-key"),
                _logger);

            await middleware.InvokeAsync(context);

            stream.Position = 0;
            var script = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("// dotnet-watch browser reload script", script);
            Assert.Contains("'some-host'", script);
            Assert.Contains("'test-key'", script);
        }

        [TestMethod]
        public async Task InvokeAsync_ConfiguresHeaders()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new BrowserScriptMiddleware(
                _next,
                new PathString("/script.js"),
                BrowserScriptMiddleware.GetWebSocketClientJavaScript("some-host", "test-key"),
                _logger);

            await middleware.InvokeAsync(context);

            var response = context.Response;
            Assert.Collection(
                response.Headers.OrderBy(h => h.Key),
                kvp =>
                {
                    Assert.AreEqual("Cache-Control", kvp.Key);
                    Assert.AreEqual("no-store", kvp.Value);
                },
                kvp =>
                {
                    Assert.AreEqual("Content-Length", kvp.Key);
                    Assert.NotEqual(0, kvp.Value.Count);
                },
                kvp =>
                {
                    Assert.AreEqual("Content-Type", kvp.Key);
                    Assert.AreEqual("application/javascript; charset=utf-8", kvp.Value);
                });
        }
    }
}
