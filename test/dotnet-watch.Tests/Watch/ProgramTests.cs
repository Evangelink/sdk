// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Watch.UnitTests
{
    public class ProgramTests(MSTestContext testContext) : DotNetWatchTestBase(testContext)
    {
        [TestMethod]
        public async Task ConsoleCancelKey()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchKitchenSink")
                .WithSource();

            var console = new TestConsole(Logger);
            var reporter = new TestReporter(Logger);

            var watching = reporter.RegisterSemaphore(MessageDescriptor.WatchingWithHotReload);
            var shutdownRequested = reporter.RegisterSemaphore(MessageDescriptor.ShutdownRequested);

            var program = Program.TryCreate(
                TestOptions.GetCommandLineOptions(["--verbose"]),
                console,
                TestOptions.GetEnvironmentOptions(workingDirectory: testAsset.Path, TestContext.Current.ToolsetUnderTest.DotNetHostPath, testAsset),
                reporter,
                out var errorCode);

            Assert.AreEqual(0, errorCode);
            Assert.IsNotNull(program);

            var run = program.RunAsync();

            await watching.WaitAsync();

            console.PressKey(new ConsoleKeyInfo('C', ConsoleKey.C, shift: false, alt: false, control: true));

            var exitCode = await run;
            Assert.AreEqual(0, exitCode);

            await shutdownRequested.WaitAsync();
        }

        [TestMethod]
        [DataRow(new[] { "--no-hot-reload", "run" }, "")]
        [DataRow(new[] { "--no-hot-reload", "run", "args" }, "args")]
        [DataRow(new[] { "--no-hot-reload", "--", "run", "args" }, "run,args")]
        [DataRow(new[] { "--no-hot-reload" }, "")]
        [DataRow(new string[] { }, "")]
        [DataRow(new[] { "run" }, "")]
        [DataRow(new[] { "run", "args" }, "args")]
        [DataRow(new[] { "--", "run", "args" }, "run,args")]
        [DataRow(new[] { "--", "test", "args" }, "test,args")]
        [DataRow(new[] { "--", "build", "args" }, "build,args")]
        [DataRow(new[] { "abc" }, "abc")]
        public async Task Arguments(string[] arguments, string expectedApplicationArgs)
        {
            var testAsset = TestAssets.CopyTestAsset("WatchHotReloadApp", identifier: string.Join(",", arguments))
                .WithSource();

            App.Start(testAsset, arguments);

            Assert.AreEqual(expectedApplicationArgs, await App.AssertOutputLineStartsWith("Arguments = "));
        }

        [TestMethod]
        [DataRow(new[] { "--no-hot-reload", "--", "run", "args" }, "Argument Specified in Props,run,args")]
        [DataRow(new[] { "--", "run", "args" }, "Argument Specified in Props,run,args")]
        // if arguments specified on command line the ones from launch profile are ignored
        [DataRow(new[] { "-lp", "P1", "--", "run", "args" },"Argument Specified in Props,run,args")]
        // arguments specified in build file override arguments in launch profile
        [DataRow(new[] { "-lp", "P1" }, "Argument Specified in Props")]
        public async Task Arguments_HostArguments(string[] arguments, string expectedApplicationArgs)
        {
            var testAsset = TestAssets.CopyTestAsset("WatchHotReloadAppCustomHost", identifier: string.Join(",", arguments))
                .WithSource();

            App.Start(testAsset, arguments);

            AssertEx.Equal(expectedApplicationArgs, await App.AssertOutputLineStartsWith("Arguments = "));
        }

        [TestMethod]
        public async Task RunArguments_NoHotReload()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchHotReloadAppMultiTfm")
                .WithSource();

            App.DotnetWatchArgs.Clear();
            App.Start(testAsset, arguments:
            [
                "--no-hot-reload",
                "run",
                "-f",
                "net6.0",
                "--property:AssemblyVersion=1.2.3.4",
                "--property",
                "AssemblyTitle= | A=B'\tC | ",
                "-v",
                "minimal",
                "--",         // the following args are application args
                "-v",
            ]);

            Assert.AreEqual("-v", await App.AssertOutputLineStartsWith("Arguments = "));
            Assert.AreEqual("WatchHotReloadAppMultiTfm, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null", await App.AssertOutputLineStartsWith("AssemblyName = "));
            Assert.AreEqual("' | A=B'\tC | '", await App.AssertOutputLineStartsWith("AssemblyTitle = "));
            Assert.AreEqual(".NETCoreApp,Version=v6.0", await App.AssertOutputLineStartsWith("TFM = "));

            // expected testContext from build (-v minimal):
            Assert.Contains(App.Process.Output, l => l.Contains("Determining projects to restore..."));

            // not expected to find verbose testContext of dotnet watch
            Assert.DoesNotContain(App.Process.Output, l => l.Contains("Working directory:"));
        }

        [TestMethod]
        public async Task RunArguments_HotReload()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchHotReloadAppMultiTfm")
                .WithSource();

            App.DotnetWatchArgs.Clear();
            App.Start(testAsset, arguments:
            [
                "run",
                "-f",         // dotnet watch does not recognize this arg -> dotnet run arg
                "net6.0",
                "--property",
                "AssemblyVersion=1.2.3.4",
                "--property",
                "AssemblyTitle= | A=B'\tC | ",
                "--",         // the following args are not dotnet run args
                "-v",         // dotnet build argument
                "minimal"
            ]);

            Assert.AreEqual("WatchHotReloadAppMultiTfm, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null", await App.AssertOutputLineStartsWith("AssemblyName = "));
            Assert.AreEqual("' | A=B'\tC | '", await App.AssertOutputLineStartsWith("AssemblyTitle = "));
            Assert.AreEqual(".NETCoreApp,Version=v6.0", await App.AssertOutputLineStartsWith("TFM = "));

            // not expected to find verbose testContext of dotnet watch
            Assert.DoesNotContain(App.Process.Output, l => l.Contains("Working directory:"));

            Assert.Contains(App.Process.Output, l => l.Contains("Hot reload enabled."));
        }

        [TestMethod]
        [DataRow("P1", "argP1")]
        [DataRow("P and Q and \"R\"", "argPQR")]
        public async Task ArgumentsFromLaunchSettings_Watch(string profileName, string expectedArgs)
        {
            var testAsset = TestAssets.CopyTestAsset("WatchAppWithLaunchSettings", identifier: profileName)
                .WithSource();

            App.Start(testAsset, arguments: new[]
            {
                "--verbose",
                "--no-hot-reload",
                "-lp",
                profileName
            });

            Assert.AreEqual(expectedArgs, await App.AssertOutputLineStartsWith("Arguments: "));

            Assert.Contains(App.Process.Output, l => l.Contains($"Found named launch profile '{profileName}'."));
            Assert.Contains(App.Process.Output, l => l.Contains("Hot Reload disabled by command line switch."));
        }

        [TestMethod]
        [DataRow("P1", "argP1")]
        [DataRow("P and Q and \"R\"", "argPQR")]
        public async Task ArgumentsFromLaunchSettings_HotReload(string profileName, string expectedArgs)
        {
            var testAsset = TestAssets.CopyTestAsset("WatchAppWithLaunchSettings", identifier: profileName)
                .WithSource();

            App.Start(testAsset, arguments: new[]
            {
                "--verbose",
                "-lp",
                profileName
            });

            Assert.AreEqual(expectedArgs, await App.AssertOutputLineStartsWith("Arguments: "));

            Assert.Contains(App.Process.Output, l => l.Contains($"Found named launch profile '{profileName}'."));
        }

        [TestMethod]
        public async Task TestCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("XunitCore")
                .WithSource();

            App.Start(testAsset, ["--verbose", "test", "--list-tests", "/p:VSTestUseMSBuildOutput=false"]);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            App.AssertOutputContains("The following Tests are available:");
            App.AssertOutputContains("    TestNamespace.VSTestXunitTests.VSTestXunitPassTest");
            App.Process.ClearOutput();

            // update file:
            var testFile = Path.Combine(testAsset.Path, "UnitTest1.cs");
            var content = File.ReadAllText(testFile, Encoding.UTF8);
            File.WriteAllText(testFile, content.Replace("VSTestXunitPassTest", "VSTestXunitPassTest2"), Encoding.UTF8);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            App.AssertOutputContains("The following Tests are available:");
            App.AssertOutputContains("    TestNamespace.VSTestXunitTests.VSTestXunitPassTest2");
        }

        [TestMethod]
        public async Task TestCommand_MultiTargeting()
        {
            var testAsset = TestAssets.CopyTestAsset("XunitMulti")
                .WithSource();

            App.Start(testAsset, ["--verbose", "test", "--framework", ToolsetInfo.CurrentTargetFramework, "--list-tests", "/p:VSTestUseMSBuildOutput=false"]);

            await App.AssertOutputLineEquals("The following Tests are available:");
            await App.AssertOutputLineEquals("    TestNamespace.VSTestXunitTests.VSTestXunitFailTestNetCoreApp");
        }

        [TestMethod]
        public async Task BuildCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchNoDepsApp")
                .WithSource();

            App.Start(testAsset, ["--verbose", "--property", "TestProperty=123", "build", "/t:TestTarget"]);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            // evaluation affected by -c option:
            Assert.Contains("TestProperty", App.Process.Output.Single(line => line.Contains("/t:GenerateWatchList")));

            App.AssertOutputContains("dotnet watch ⌚ Command 'build' does not support Hot Reload.");
            App.AssertOutputContains("dotnet watch ⌚ Command 'build' does not support browser refresh.");
            App.AssertOutputContains("warning : The value of property is '123'");
        }

        [TestMethod]
        public async Task MSBuildCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchNoDepsApp")
                .WithSource();

            App.Start(testAsset, ["--verbose", "/p:TestProperty=123", "msbuild", "/t:TestTarget"]);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            // TestProperty is not passed to evaluation since msbuild command doesn't include it in forward options:
            Assert.DoesNotContain("TestProperty", App.Process.Output.Single(line => line.Contains("/t:GenerateWatchList")));

            App.AssertOutputContains("dotnet watch ⌚ Command 'msbuild' does not support Hot Reload.");
            App.AssertOutputContains("dotnet watch ⌚ Command 'msbuild' does not support browser refresh.");
            App.AssertOutputContains("warning : The value of property is '123'");
        }

        [TestMethod]
        public async Task PackCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchNoDepsApp")
                .WithSource();

            App.Start(testAsset, ["--verbose", "pack", "-c", "Release"]);

            var packagePath = Path.Combine(testAsset.Path, "bin", "Release", "WatchNoDepsApp.1.0.0.nupkg");

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            // evaluation affected by -c option:
            Assert.Contains("-property:Configuration=Release", App.Process.Output.Single(line => line.Contains("/t:GenerateWatchList")));

            App.AssertOutputContains("dotnet watch ⌚ Command 'pack' does not support Hot Reload.");
            App.AssertOutputContains("dotnet watch ⌚ Command 'pack' does not support browser refresh.");
            App.AssertOutputContains($"Successfully created package '{packagePath}'");
        }

        [TestMethod]
        public async Task PublishCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchNoDepsApp")
                .WithSource();

            App.Start(testAsset, ["--verbose", "publish", "-c", "Release"]);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            // evaluation affected by -c option:
            Assert.Contains("-property:Configuration=Release", App.Process.Output.Single(line => line.Contains("/t:GenerateWatchList")));

            App.AssertOutputContains("dotnet watch ⌚ Command 'publish' does not support Hot Reload.");
            App.AssertOutputContains("dotnet watch ⌚ Command 'publish' does not support browser refresh.");

            App.AssertOutputContains(Path.Combine("Release", ToolsetInfo.CurrentTargetFramework, "publish"));
        }

        [TestMethod]
        public async Task FormatCommand()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchNoDepsApp")
                .WithSource();

            App.DotnetWatchArgs.Clear();
            App.Start(testAsset, ["--verbose", "format", "--verbosity", "detailed"]);

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            App.AssertOutputContains("dotnet watch ⌚ Command 'format' does not support Hot Reload.");
            App.AssertOutputContains("dotnet watch ⌚ Command 'format' does not support browser refresh.");

            App.AssertOutputContains("format --verbosity detailed");
            App.AssertOutputContains("Format complete in");
        }

        [TestMethod]
        public async Task ProjectGraphLoadFailure()
        {
            var testAsset = TestAssets
                .CopyTestAsset("WatchAppWithProjectDeps")
                .WithSource()
                .WithProjectChanges((path, proj) =>
                {
                    if (Path.GetFileName(path) == "App.WithDeps.csproj")
                    {
                        proj.Root.Descendants()
                            .Single(e => e.Name.LocalName == "ItemGroup")
                            .Add(XElement.Parse("""
                            <ProjectReference Include="NonExistentDirectory\X.csproj" />
                            """));
                    }
                });

            App.Start(testAsset, [], "AppWithDeps");

            await App.AssertOutputLineStartsWith("dotnet watch ⌚ Fix the error to continue or press Ctrl+C to exit.");

            App.AssertOutputContains(@"dotnet watch ⌚ Failed to load project graph.");
            App.AssertOutputContains($"dotnet watch ❌ The project file could not be loaded. Could not find a part of the path '{Path.Combine(testAsset.Path, "AppWithDeps", "NonExistentDirectory", "X.csproj")}'");
        }
    }
}
