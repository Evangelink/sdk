// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.DotNet.Cli.Telemetry;
using Microsoft.DotNet.Cli.Utils;
using Parser = Microsoft.DotNet.Cli.Parser;

namespace Microsoft.DotNet.Tests
{
    /// <summary>
    /// Only adding the performance data tests for now as the TelemetryCommandTests cover most other scenarios already
    /// </summary>
    public class TelemetryFilterTests : SdkTest
    {
        private readonly FakeRecordEventNameTelemetry _fakeTelemetry;

        public string EventName { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        public TelemetryFilterTests(MSTestContext testContext) : base(testContext)
        {
            _fakeTelemetry = new FakeRecordEventNameTelemetry();
            TelemetryEventEntry.Subscribe(_fakeTelemetry.TrackEvent);
            TelemetryEventEntry.TelemetryFilter = new TelemetryFilter(Sha256Hasher.HashWithNormalizedCasing);
        }

        [TestMethod]
        public void TopLevelCommandNameShouldBeSentToTelemetryWithoutPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "build" });
            TelemetryEventEntry.SendFiltered(parseResult);
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "toplevelparser/command" &&
                  e.Properties.ContainsKey("verb") &&
                  e.Properties["verb"] == Sha256Hasher.Hash("BUILD") &&
                  e.Measurement == null);
        }

        [TestMethod]
        public void TopLevelCommandNameShouldBeSentToTelemetryWithPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "build" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 12345 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "toplevelparser/command" &&
                  e.Properties.ContainsKey("verb") &&
                  e.Properties["verb"] == Sha256Hasher.Hash("BUILD") &&
                  e.Measurement.ContainsKey("Startup Time") &&
                  e.Measurement["Startup Time"] == 12345);
        }

        [TestMethod]
        public void TopLevelCommandNameShouldBeSentToTelemetryWithZeroPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "build" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 0 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "toplevelparser/command" &&
                  e.Properties.ContainsKey("verb") &&
                  e.Properties["verb"] == Sha256Hasher.Hash("BUILD") &&
                  e.Measurement == null);
        }

        [TestMethod]
        public void TopLevelCommandNameShouldBeSentToTelemetryWithSomeZeroPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "build" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 23456 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "toplevelparser/command" &&
                  e.Properties.ContainsKey("verb") &&
                  e.Properties["verb"] == Sha256Hasher.Hash("BUILD") &&
                  !e.Measurement.ContainsKey("Startup Time") &&
                  e.Measurement.ContainsKey("Parse Time") &&
                  e.Measurement["Parse Time"] == 23456);
        }

        [TestMethod]
        public void SubLevelCommandNameShouldBeSentToTelemetryWithoutPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "new", "console" });
            TelemetryEventEntry.SendFiltered(parseResult);
            _fakeTelemetry
                .LogEntries.Should()
                .Contain(e => e.EventName == "sublevelparser/command" &&
                    e.Properties.ContainsKey("argument") &&
                    e.Properties["argument"] == Sha256Hasher.Hash("CONSOLE") &&
                    e.Properties.ContainsKey("verb") &&
                    e.Properties["verb"] == Sha256Hasher.Hash("NEW") &&
                    e.Measurement == null);
        }

        [TestMethod]
        public void SubLevelCommandNameShouldBeSentToTelemetryWithPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "new", "console" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 34567 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                    e.Properties.ContainsKey("argument") &&
                    e.Properties["argument"] == Sha256Hasher.Hash("CONSOLE") &&
                    e.Properties.ContainsKey("verb") &&
                    e.Properties["verb"] == Sha256Hasher.Hash("NEW") &&
                    e.Measurement.ContainsKey("Startup Time") &&
                    e.Measurement["Startup Time"] == 34567);
        }

        [TestMethod]
        public void SubLevelCommandNameShouldBeSentToTelemetryWithZeroPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "new", "console" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 0 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                    e.Properties.ContainsKey("argument") &&
                    e.Properties["argument"] == Sha256Hasher.Hash("CONSOLE") &&
                    e.Properties.ContainsKey("verb") &&
                    e.Properties["verb"] == Sha256Hasher.Hash("NEW") &&
                    e.Measurement == null);
        }

        [TestMethod]
        public void SubLevelCommandNameShouldBeSentToTelemetryWithSomeZeroPerformanceData()
        {
            var parseResult = Parser.Instance.Parse(new List<string>() { "new", "console" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult, new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 45678 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                    e.Properties.ContainsKey("argument") &&
                    e.Properties["argument"] == Sha256Hasher.Hash("CONSOLE") &&
                    e.Properties.ContainsKey("verb") &&
                    e.Properties["verb"] == Sha256Hasher.Hash("NEW") &&
                    !e.Measurement.ContainsKey("Startup Time") &&
                    e.Measurement.ContainsKey("Parse Time") &&
                    e.Measurement["Parse Time"] == 45678);
        }

        [TestMethod]
        public void WorkloadSubLevelCommandNameAndArgumentShouldBeSentToTelemetry()
        {
            var parseResult =
                Parser.Instance.Parse(new List<string>() { "workload", "install", "microsoft-ios-sdk-full" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult,
                new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 23456 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                                                            e.Properties.ContainsKey("verb") &&
                                                            e.Properties["verb"] == Sha256Hasher.Hash("WORKLOAD") &&
                                                            e.Properties["subcommand"] ==
                                                            Sha256Hasher.Hash("INSTALL") &&
                                                            e.Properties["argument"] ==
                                                            Sha256Hasher.Hash("MICROSOFT-IOS-SDK-FULL"));
        }

        [TestMethod]
        public void ToolsSubLevelCommandNameAndArgumentShouldBeSentToTelemetry()
        {
            var parseResult =
                Parser.Instance.Parse(new List<string>() { "tool", "install", "dotnet-format" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult,
                new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 23456 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                                                            e.Properties.ContainsKey("verb") &&
                                                            e.Properties["verb"] == Sha256Hasher.Hash("TOOL") &&
                                                            e.Properties["subcommand"] ==
                                                            Sha256Hasher.Hash("INSTALL") &&
                                                            e.Properties["argument"] ==
                                                            Sha256Hasher.Hash("DOTNET-FORMAT"));
        }

        [TestMethod]
        public void WhenCalledWithDiagnosticWorkloadSubLevelCommandNameAndArgumentShouldBeSentToTelemetry()
        {
            var parseResult =
                Parser.Instance.Parse(new List<string>() { "-d", "workload", "install", "microsoft-ios-sdk-full" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult,
                new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 23456 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                                                            e.Properties.ContainsKey("verb") &&
                                                            e.Properties["verb"] == Sha256Hasher.Hash("WORKLOAD") &&
                                                            e.Properties["subcommand"] ==
                                                            Sha256Hasher.Hash("INSTALL") &&
                                                            e.Properties["argument"] ==
                                                            Sha256Hasher.Hash("MICROSOFT-IOS-SDK-FULL"));
        }

        [TestMethod]
        public void WhenCalledWithMissingArgumentWorkloadSubLevelCommandNameAndArgumentShouldBeSentToTelemetry()
        {
            var parseResult =
                Parser.Instance.Parse(new List<string>() { "-d", "workload", "install" });
            TelemetryEventEntry.SendFiltered(Tuple.Create(parseResult,
                new Dictionary<string, double>() { { "Startup Time", 0 }, { "Parse Time", 23456 } }));
            _fakeTelemetry.LogEntries.Should().Contain(e => e.EventName == "sublevelparser/command" &&
                                                            e.Properties.ContainsKey("verb") &&
                                                            e.Properties["verb"] == Sha256Hasher.Hash("WORKLOAD") &&
                                                            e.Properties["subcommand"] ==
                                                            Sha256Hasher.Hash("INSTALL"));
        }
    }
}
