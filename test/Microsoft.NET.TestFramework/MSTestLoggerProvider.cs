// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Microsoft.NET.TestFramework
{
    // <summary>
    /// Microsoft.Extensions.Logging <see cref="ILoggerProvider"/> which logs to XUnit test testContext.
    /// </summary>
    /// <remarks>
    /// See https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging/tests/DI.Common/Common/src/XunitLoggerProvider.cs for more details.
    /// </remarks>
    public class MSTestLoggerProvider : ILoggerProvider
    {
        private readonly MSTestTestContext _testContext;
        private readonly LogLevel _minLevel;
        private readonly DateTimeOffset? _testContextStart;

        public MSTestLoggerProvider(MSTestTestContext testContext)
            : this(testContext, LogLevel.Trace)
        {
        }

        public MSTestLoggerProvider(MSTestTestContext testContext, LogLevel minLevel)
            : this(testContext, minLevel, null)
        {
        }

        public MSTestLoggerProvider(MSTestTestContext testContext, LogLevel minLevel, DateTimeOffset? logStart)
        {
            _testContext = testContext;
            _minLevel = minLevel;
            _testContextStart = logStart;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MSTestLogger(_testContext, categoryName, _minLevel, _testContextStart);
        }

        public void Dispose()
        {
        }

        private class MSTestLogger : ILogger
        {
            private static readonly string[] NewLineChars = new[] { Environment.NewLine };
            private readonly string _category;
            private readonly LogLevel _minLogLevel;
            private readonly MSTestTestContext _testContext;
            private readonly DateTimeOffset? _testContextStart;

            public MSTestLogger(MSTestTestContext testContext, string category, LogLevel minLogLevel, DateTimeOffset? logStart)
            {
                _minLogLevel = minLogLevel;
                _category = category;
                _testContext = testContext;
                _testContextStart = logStart;
            }

            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                // Buffer the message into a single string in order to avoid shearing the message when running across multiple threads.
                var messageBuilder = new StringBuilder();

                var timestamp = _testContextStart.HasValue ? $"{(DateTimeOffset.UtcNow - _testContextStart.Value).TotalSeconds:N3}s" : DateTimeOffset.UtcNow.ToString("s");

                var firstLinePrefix = $"| [{timestamp}] {_category} {logLevel}: ";
                var lines = formatter(state, exception).Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                messageBuilder.AppendLine(firstLinePrefix + lines.FirstOrDefault() ?? string.Empty);

                var additionalLinePrefix = "|" + new string(' ', firstLinePrefix.Length - 1);
                foreach (var line in lines.Skip(1))
                {
                    messageBuilder.AppendLine(additionalLinePrefix + line);
                }

                if (exception != null)
                {
                    lines = exception.ToString().Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    additionalLinePrefix = "| ";
                    foreach (var line in lines)
                    {
                        messageBuilder.AppendLine(additionalLinePrefix + line);
                    }
                }

                // Remove the last line-break, because ITestOutputHelper only has WriteLine.
                var message = messageBuilder.ToString();
                if (message.EndsWith(Environment.NewLine))
                {
                    message = message.Substring(0, message.Length - Environment.NewLine.Length);
                }

                try
                {
                    _testContext.WriteLine(message);
                }
                catch (Exception)
                {
                    // We could fail because we're on a background thread and our captured ITestOutputHelper is
                    // busted (if the test "completed" before the background thread fired).
                    // So, ignore this. There isn't really anything we can do but hope the
                    // caller has additional loggers registered
                }
            }

            public bool IsEnabled(LogLevel logLevel)
                => logLevel >= _minLogLevel;

            IDisposable ILogger.BeginScope<TState>(TState state) => new NullScope();

            private class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
