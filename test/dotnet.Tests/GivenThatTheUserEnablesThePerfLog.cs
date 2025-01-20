// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Tracing;

namespace Microsoft.DotNet.Tests
{
    public class GivenThatTheUserEnablesThePerfLog : SdkTest
    {
        public GivenThatTheUserEnablesThePerfLog(MSTestContext testContext) : base(testContext)
        {
        }

        [TestMethod]
        public void WhenPerfLogDisabledDotNetDoesNotWriteToThePerfLog()
        {
            var dir = _testAssetsManager.CreateTestDirectory();

            var result = new DotnetCommand(Log, "--help")
                .WithEnvironmentVariable("DOTNET_PERFLOG_DIR", dir.Path)
                .Execute();

            result.ExitCode.Should().Be(0);
            Assert.Empty(new DirectoryInfo(dir.Path).GetFiles());
        }

        [TestMethod]
        public void WhenPerfLogEnabledDotNetWritesToThePerfLog()
        {
            var dir = _testAssetsManager.CreateTestDirectory();

            var result = new DotnetCommand(Log, "--help")
                .WithEnvironmentVariable("DOTNET_CLI_PERF_LOG", "1")
                .WithEnvironmentVariable("DOTNET_PERFLOG_DIR", dir.Path)
                .Execute();

            result.ExitCode.Should().Be(0);

            DirectoryInfo logDir = new(dir.Path);
            FileInfo[] logFiles = logDir.GetFiles();
            Assert.NotEmpty(logFiles);
            Assert.All(logFiles, f => Assert.StartsWith("perf-", f.Name));
            Assert.All(logFiles, f => Assert.NotEqual(0, f.Length));
        }

        [TestMethod]
        public void WhenPerfLogEnabledDotNetBuildWritesAPerfLog()
        {
            using (PerfLogTestEventListener listener = new())
            {
                int exitCode = Cli.Program.Main(new string[] { "--help" });
                Assert.AreEqual(0, exitCode);
                Assert.NotEqual(0, listener.EventCount);
            }
        }
    }

    internal sealed class PerfLogTestEventListener : EventListener
    {
        private const string PerfLogEventSourceName = "Microsoft-Dotnet-CLI-Performance";

        public int EventCount
        {
            get; private set;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name.Equals(PerfLogEventSourceName))
            {
                EnableEvents(eventSource, EventLevel.Verbose);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Assert.AreEqual(PerfLogEventSourceName, eventData.EventSource.Name);
            EventCount++;
        }
    }
}
