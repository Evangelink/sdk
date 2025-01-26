// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Watch.UnitTests
{
    [TestClass]
    public class CommandLineOptionsTests
    {
        private readonly MockReporter _testReporter = new();

        private CommandLineOptions VerifyOptions(string[] args, string expectedOutput = "", string[] expectedMessages = null)
            => VerifyOptions(args, actualOutput => AssertEx.Equal(expectedOutput, actualOutput), expectedMessages ?? []);

        private CommandLineOptions VerifyOptions(string[] args, Action<string> outputValidator, string[] expectedMessages)
        {
            var output = new StringWriter();
            var options = CommandLineOptions.Parse(args, _testReporter, output: output, errorCode: out var errorCode);

            Assert.AreEqual(expectedMessages, _testReporter.Messages);
            outputValidator(output.ToString());

            Assert.IsNotNull(options);
            Assert.AreEqual(0, errorCode);
            return options;
        }

        private void VerifyErrors(string[] args, params string[] expectedErrors)
        {
            var output = new StringWriter();
            var options = CommandLineOptions.Parse(args, _testReporter, output: output, errorCode: out var errorCode);

            AssertEx.Equal(expectedErrors, _testReporter.Messages);
            Assert.IsEmpty(output.ToString());

            Assert.IsNull(options);
            Assert.AreNotEqual(0, errorCode);
        }

        [TestMethod]
        [DataRow(new object[] { new[] { "-h" } })]
        [DataRow(new object[] { new[] { "-?" } })]
        [DataRow(new object[] { new[] { "--help" } })]
        [DataRow(new object[] { new[] { "--help", "--bogus" } })]
        public void HelpArgs(string[] args)
        {
            var output = new StringWriter();
            var options = CommandLineOptions.Parse(args, _testReporter, output: output, errorCode: out var errorCode);
            Assert.IsNull(options);
            Assert.AreEqual(0, errorCode);

            Assert.IsEmpty(_testReporter.Messages);
            Assert.Contains("Usage:", output.ToString());
        }

        [TestMethod]
        [DataRow("-p:P=V", "P", "V")]
        [DataRow("-p:P==", "P", "=")]
        [DataRow("-p:P=A=B", "P", "A=B")]
        [DataRow("-p: P\t = V ", "P", " V ")]
        [DataRow("-p:P=", "P", "")]
        public void BuildProperties_Valid(string argValue, string name, string value)
        {
            var properties = CommandLineOptions.ParseBuildProperties([argValue]);
            AssertEx.SequenceEqual([(name, value)], properties);
        }

        [TestMethod]
        [DataRow("P")]
        [DataRow("=P3")]
        [DataRow("=")]
        [DataRow("==")]
        public void BuildProperties_Invalid(string argValue)
        {
            var properties = CommandLineOptions.ParseBuildProperties([argValue]);
            AssertEx.SequenceEqual([], properties);
        }

        [TestMethod]
        public void ImplicitCommand()
        {
            var options = VerifyOptions([]);
            Assert.AreEqual("run", options.Command);
            Assert.IsEmpty(options.CommandArguments);
        }

        [TestMethod]
        [DataRow("add")]
        [DataRow("build")]
        [DataRow("build-server")]
        [DataRow("clean")]
        [DataRow("format")]
        [DataRow("help")]
        [DataRow("list")]
        [DataRow("msbuild")]
        [DataRow("new")]
        [DataRow("nuget")]
        [DataRow("pack")]
        [DataRow("publish")]
        [DataRow("remove")]
        [DataRow("restore")]
        [DataRow("run")]
        [DataRow("sdk")]
        [DataRow("solution")]
        [DataRow("store")]
        [DataRow("test")]
        [DataRow("tool")]
        [DataRow("vstest")]
        [DataRow("workload")]
        public void ExplicitCommand(string command)
        {
            var options = VerifyOptions([command]);
            Assert.AreEqual(command, options.ExplicitCommand);
            Assert.AreEqual(command, options.Command);
            Assert.IsEmpty(options.CommandArguments);
        }

        [TestMethod]
        [CombinatorialData]
        public void WatchOptions_NotPassedThrough_BeforeCommand(
            [CombinatorialValues("--quiet", "--verbose", "--no-hot-reload", "--non-interactive")] string option,
            bool before)
        {
            var options = VerifyOptions(before ? [option, "test"] : ["test", option]);
            Assert.AreEqual("test", options.Command);
            Assert.IsEmpty(options.CommandArguments);
        }

        [TestMethod]
        public void RunOptions_LaunchProfile_Watch()
        {
            var options = VerifyOptions(["-lp", "P", "run"]);
            Assert.AreEqual("P", options.LaunchProfileName);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["-lp", "P"], options.CommandArguments);
        }

        [TestMethod]
        public void RunOptions_LaunchProfile_Run()
        {
            var options = VerifyOptions(["run", "-lp", "P"]);
            Assert.AreEqual("P", options.LaunchProfileName);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["-lp", "P"], options.CommandArguments);
        }

        [TestMethod]
        public void RunOptions_LaunchProfile_Both()
        {
            VerifyErrors(["-lp", "P1", "run", "-lp", "P2"],
                "error ❌ Option '-lp' expects a single argument but 2 were provided.");
        }

        [TestMethod]
        public void RunOptions_NoProfile_Watch()
        {
            var options = VerifyOptions(["--no-launch-profile", "run"]);

            Assert.IsTrue(options.NoLaunchProfile);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["--no-launch-profile"], options.CommandArguments);
        }

        [TestMethod]
        public void RunOptions_NoProfile_Run()
        {
            var options = VerifyOptions(["run", "--no-launch-profile"]);

            Assert.IsTrue(options.NoLaunchProfile);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["--no-launch-profile"], options.CommandArguments);
        }

        [TestMethod]
        public void RunOptions_NoProfile_Both()
        {
            var options = VerifyOptions(["--no-launch-profile", "run", "--no-launch-profile"]);

            Assert.IsTrue(options.NoLaunchProfile);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["--no-launch-profile"], options.CommandArguments);
        }

        [TestMethod]
        public void RemainingOptions()
        {
            var options = VerifyOptions(["-watchArg", "--verbose", "run", "-runArg"]);

            Assert.IsTrue(options.GlobalOptions.Verbose);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["-watchArg", "-runArg"], options.CommandArguments);
        }

        [TestMethod]
        public void UnknownOption()
        {
            var options = VerifyOptions(["--verbose", "--unknown", "x", "y", "run", "--project", "p"]);

            Assert.AreEqual("p", options.ProjectPath);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["--project", "p", "--unknown", "x", "y"], options.CommandArguments);
        }

        [TestMethod]
        public void RemainingOptionsDashDash()
        {
            var options = VerifyOptions(["-watchArg", "--", "--verbose", "run", "-runArg"]);

            Assert.IsFalse(options.GlobalOptions.Verbose);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["-watchArg", "--", "--verbose", "run", "-runArg"], options.CommandArguments);
        }

        [TestMethod]
        public void RemainingOptionsDashDashRun()
        {
            var options = VerifyOptions(["--", "run"]);

            Assert.IsFalse(options.GlobalOptions.Verbose);
            Assert.AreEqual("run", options.Command);
            Assert.AreEqual(["--", "run"], options.CommandArguments);
        }

        [TestMethod]
        public void NoOptionsAfterDashDash()
        {
            var options = VerifyOptions(["--"]);
            Assert.AreEqual("run", options.Command);
            Assert.IsEmpty(options.CommandArguments);
        }

        /// <summary>
        /// dotnet watch needs to understand some options that are passed to the subcommands.
        /// For example, `-f TFM`
        /// When `dotnet watch run -- -f TFM` is parsed `-f TFM` is ignored.
        /// Therfore, it has to also be ignored by `dotnet run`,
        /// otherwise the TFMs would be inconsistent between `dotnet watch` and `dotnet run`.
        /// </summary>
        [TestMethod]
        public void ParsedNonWatchOptionsAfterDashDash_Framework()
        {
            var options = VerifyOptions(["--", "-f", "TFM"]);

            Assert.IsNull(options.TargetFramework);
            Assert.AreEqual(["--", "-f", "TFM"], options.CommandArguments);
        }

        [TestMethod]
        public void ParsedNonWatchOptionsAfterDashDash_Project()
        {
            var options = VerifyOptions(["--", "--project", "proj"]);

            Assert.IsNull(options.ProjectPath);
            Assert.AreEqual(["--", "--project", "proj"], options.CommandArguments);
        }

        [TestMethod]
        public void ParsedNonWatchOptionsAfterDashDash_NoLaunchProfile()
        {
            var options = VerifyOptions(["--", "--no-launch-profile"]);

            Assert.IsFalse(options.NoLaunchProfile);
            Assert.AreEqual(["--", "--no-launch-profile"], options.CommandArguments);
        }

        [TestMethod]
        public void ParsedNonWatchOptionsAfterDashDash_LaunchProfile()
        {
            var options = VerifyOptions(["--", "--launch-profile", "p"]);

            Assert.IsFalse(options.NoLaunchProfile);
            Assert.AreEqual(["--", "--launch-profile", "p"], options.CommandArguments);
        }

        [TestMethod]
        public void ParsedNonWatchOptionsAfterDashDash_Property()
        {
            var options = VerifyOptions(["--", "--property", "x=1"]);

            Assert.IsFalse(options.NoLaunchProfile);
            Assert.AreEqual(["--", "--property", "x=1"], options.CommandArguments);
        }

        [TestMethod]
        [CombinatorialData]
        public void OptionsSpecifiedBeforeOrAfterRun(bool afterRun)
        {
            var args = new[] { "--project", "P", "--framework", "F", "--property", "P1=V1", "--property", "P2=V2" };
            args = afterRun ? args.Prepend("run").ToArray() : args.Append("run").ToArray();

            var options = VerifyOptions(args);

            Assert.AreEqual("P", options.ProjectPath);
            Assert.AreEqual("F", options.TargetFramework);
            Assert.AreEqual(["-property:TargetFramework=F", "--property:P1=V1", "--property:P2=V2"], options.BuildArguments);

            Assert.AreEqual(["--project", "P", "--framework", "F", "--property:P1=V1", "--property:P2=V2"], options.CommandArguments);
        }

        public enum ArgPosition
        {
            Before,
            After,
            Both
        }

        [TestMethod]
        [CombinatorialData]
        public void OptionDuplicates_Allowed_Bool(
            ArgPosition position,
            [CombinatorialValues(
                "--verbose",
                "--quiet",
                "--list",
                "--no-hot-reload",
                "--non-interactive")]
            string arg)
        {
            var args = new[] { arg };

            args = position switch
            {
                ArgPosition.Before => args.Prepend("run").ToArray(),
                ArgPosition.Both => args.Concat(new[] { "run" }).Concat(args).ToArray(),
                ArgPosition.After => args.Append("run").ToArray(),
                _ => args,
            };

            var options = VerifyOptions(args);

            Assert.IsTrue(arg switch
            {
                "--verbose" => options.GlobalOptions.Verbose,
                "--quiet" => options.GlobalOptions.Quiet,
                "--list" => options.List,
                "--no-hot-reload" => options.GlobalOptions.NoHotReload,
                "--non-interactive" => options.GlobalOptions.NonInteractive,
                _ => false
            });
        }

        [TestMethod]
        public void MultiplePropertyValues()
        {
            var options = VerifyOptions(["--property", "P1=V1", "run", "--property", "P2=V2"]);
            AssertEx.SequenceEqual(["--property:P1=V1", "--property:P2=V2"], options.BuildArguments);

            // options must be repeated since --property does not support multiple args
            AssertEx.SequenceEqual(["--property:P1=V1", "--property:P2=V2"], options.CommandArguments);
        }

        [TestMethod]
        [DataRow("--project")]
        [DataRow("--framework")]
        public void OptionDuplicates_NotAllowed(string option)
        {
            VerifyErrors([option, "abc", "run", option, "xyz"],
                $"error ❌ Option '{option}' expects a single argument but 2 were provided.");
        }

        [TestMethod]
        [DataRow(new[] { "--unrecognized-arg" }, new[] { "--unrecognized-arg" })]
        [DataRow(new[] { "run" }, new string[] { })]
        [DataRow(new[] { "run", "--", "runarg" }, new[] { "--", "runarg" })]
        [DataRow(new[] { "--verbose", "run", "runarg1", "-runarg2" }, new[] { "runarg1", "-runarg2" })]
        // run is after -- and therefore not parsed as a command:
        [DataRow(new[] { "--verbose", "--", "run", "--", "runarg" }, new[] { "--", "run", "--", "runarg" })]
        // run is before -- and therefore parsed as a command:
        [DataRow(new[] { "--verbose", "run", "--", "--", "runarg" }, new[] { "--", "--", "runarg" })]
        public void ParsesRemainingArgs(string[] args, string[] expected)
        {
            var options = VerifyOptions(args);
            Assert.AreEqual(expected, options.CommandArguments);
        }

        [TestMethod]
        public void CannotHaveQuietAndVerbose()
        {
            VerifyErrors(["--quiet", "--verbose"],
                $"error ❌ {Resources.Error_QuietAndVerboseSpecified}");
        }

        [TestMethod]
        public void ShortFormForProjectArgumentPrintsWarning()
        {
            var options = VerifyOptions(["-p", "MyProject.csproj"],
                expectedMessages: [$"warning ⌚ {Resources.Warning_ProjectAbbreviationDeprecated}"]);

            Assert.AreEqual("MyProject.csproj", options.ProjectPath);
        }

        [TestMethod]
        public void LongFormForProjectArgumentWorks()
        {
            var options = VerifyOptions(["--project", "MyProject.csproj"]);
            Assert.AreEqual("MyProject.csproj", options.ProjectPath);
        }

        [TestMethod]
        public void LongFormForLaunchProfileArgumentWorks()
        {
            var options = VerifyOptions(["--launch-profile", "CustomLaunchProfile"]);
            Assert.IsNotNull(options);
            Assert.AreEqual("CustomLaunchProfile", options.LaunchProfileName);
        }

        [TestMethod]
        public void ShortFormForLaunchProfileArgumentWorks()
        {
            var options = VerifyOptions(["-lp", "CustomLaunchProfile"]);
            Assert.AreEqual("CustomLaunchProfile", options.LaunchProfileName);
        }

        /// <summary>
        /// Validates that options that the "run" command forwards to "build" command are forwarded by dotnet-watch.
        /// </summary>
        [TestMethod]
        [DataRow(new[] { "--configuration", "release" }, new[] { "-property:Configuration=release" })]
        [DataRow(new[] { "--framework", "net9.0" }, new[] { "-property:TargetFramework=net9.0" })]
        [DataRow(new[] { "--runtime", "arm64" }, new[] { "-property:RuntimeIdentifier=arm64", "-property:_CommandLineDefinedRuntimeIdentifier=true" })]
        [DataRow(new[] { "--property", "b=1" }, new[] { "--property:b=1" })]
        [DataRow(new[] { "--interactive" }, new[] { "-property:NuGetInteractive=true" })]
        [DataRow(new[] { "--no-restore" }, new[] { "-restore:false" })]
        [DataRow(new[] { "--sc" }, new[] { "-property:SelfContained=True", "-property:_CommandLineDefinedSelfContained=true" })]
        [DataRow(new[] { "--self-contained" }, new[] { "-property:SelfContained=True", "-property:_CommandLineDefinedSelfContained=true" })]
        [DataRow(new[] { "--no-self-contained" }, new[] { "-property:SelfContained=False", "-property:_CommandLineDefinedSelfContained=true" })]
        [DataRow(new[] { "--verbosity", "q" }, new[] { "-verbosity:q" })]
        [DataRow(new[] { "--arch", "arm", "--os", "win" }, new[] { "-property:RuntimeIdentifier=win-arm" })]
        [DataRow(new[] { "--disable-build-servers" }, new[] { "--property:UseRazorBuildServer=false", "--property:UseSharedCompilation=false", "/nodeReuse:false" })]
        public void ForwardedBuildOptions(string[] args, string[] buildArgs)
        {
            var options = VerifyOptions(["run", .. args]);
            AssertEx.SequenceEqual(buildArgs, options.BuildArguments);
        }

        [TestMethod]
        public void ForwardedBuildOptions_ArtifactsPath()
        {
            var path = TestContext.Current.TestAssetsDirectory;

            var args = new[] { "--artifacts-path", path };
            var buildArgs = new[] { @"-property:ArtifactsPath=" + path };

            var options = VerifyOptions(["run", .. args]);
            AssertEx.SequenceEqual(buildArgs, options.BuildArguments);
        }
    }
}
