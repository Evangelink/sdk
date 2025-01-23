// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Watch.UnitTests
{
    public class ReporterTests
    {
        private static readonly string EOL = Environment.NewLine;

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void WritesToStandardStreams(bool suppressEmojis)
        {
            var testConsole = new TestConsole();
            IReporter reporter = new ConsoleReporter(testConsole, verbose: true, quiet: false, suppressEmojis: suppressEmojis);
            var dotnetWatchDefaultPrefix = $"dotnet watch {(suppressEmojis ? ":" : "⌚")} ";

            // stdout
            reporter.Verbose("verbose {0}");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix}verbose {{0}}" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            reporter.Output("out");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix}out" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            reporter.Warn("warn");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix}warn" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            // stderr
            reporter.Error("error");
            Assert.AreEqual($"dotnet watch {(suppressEmojis ? ":" : "❌")} error" + EOL, testConsole.GetError());
            testConsole.Clear();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void WritesToStandardStreamsWithCustomEmojis(bool suppressEmojis)
        {
            var testConsole = new TestConsole();
            IReporter reporter = new ConsoleReporter(testConsole, verbose: true, quiet: false, suppressEmojis: suppressEmojis);
            var dotnetWatchDefaultPrefix = $"dotnet watch {(suppressEmojis ? ":" : "😄")}";

            // stdout
            reporter.Verbose("verbose", emoji: "😄");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix} verbose" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            reporter.Output("out", emoji: "😄");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix} out" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            reporter.Warn("warn", emoji: "😄");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix} warn" + EOL, testConsole.GetOutput());
            testConsole.Clear();

            // stderr
            reporter.Error("error", emoji: "😄");
            Assert.AreEqual($"{dotnetWatchDefaultPrefix} error" + EOL, testConsole.GetError());
            testConsole.Clear();
        }

        private class TestConsole : IConsole
        {
            private readonly StringBuilder _out;
            private readonly StringBuilder _error;
            public TextWriter Out { get; }
            public TextWriter Error { get; }
            public ConsoleColor ForegroundColor { get; set; }

            public TestConsole()
            {
                _out = new StringBuilder();
                _error = new StringBuilder();
                Out = new StringWriter(_out);
                Error = new StringWriter(_error);
            }

            event Action<ConsoleKeyInfo> IConsole.KeyPressed
            {
                add { }
                remove { }
            }

            public string GetOutput()
                => _out.ToString();

            public string GetError()
                => _error.ToString();

            public void Clear()
            {
                _out.Clear();
                _error.Clear();
            }

            public void ResetColor()
            {
                ForegroundColor = default;
            }
        }
    }
}
