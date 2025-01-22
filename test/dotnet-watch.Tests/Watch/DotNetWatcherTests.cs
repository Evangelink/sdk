// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Globalization;

namespace Microsoft.DotNet.Watch.UnitTests
{
    public class DotNetWatcherTests : DotNetWatchTestBase
    {
        private const string AppName = "WatchKitchenSink";

        public DotNetWatcherTests(MSTestContext testContext)
            : base(logger)
        {
        }

        [TestMethod]
        public async Task RunsWithDotnetWatchEnvVariable()
        {
            Assert.IsTrue(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH")), "DOTNET_WATCH cannot be set already when this test is running");

            var testAsset = TestAssets.CopyTestAsset(AppName)
                .WithSource();

            App.Start(testAsset, []);
            Assert.AreEqual("1", await App.AssertOutputLineStartsWith("DOTNET_WATCH = "));
        }

        [TestMethod]
        [CombinatorialData]
        public async Task RunsWithDotnetLaunchProfileEnvVariableWhenNotExplicitlySpecified(bool hotReload)
        {
            Assert.IsTrue(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE")), "DOTNET_LAUNCH_PROFILE cannot be set already when this test is running");

            var testAsset = TestAssets.CopyTestAsset(AppName, identifier: hotReload.ToString())
                .WithSource();

            if (!hotReload)
            {
                App.DotnetWatchArgs.Add("--no-hot-reload");
            }

            App.Start(testAsset, []);
            Assert.AreEqual("<<<First>>>", await App.AssertOutputLineStartsWith("DOTNET_LAUNCH_PROFILE = "));
        }

        [TestMethod]
        [CombinatorialData]
        public async Task RunsWithDotnetLaunchProfileEnvVariableWhenExplicitlySpecified(bool hotReload)
        {
            Assert.IsTrue(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE")), "DOTNET_LAUNCH_PROFILE cannot be set already when this test is running");

            var testAsset = TestAssets.CopyTestAsset(AppName, identifier: hotReload.ToString())
                .WithSource();

            if (!hotReload)
            {
                App.DotnetWatchArgs.Add("--no-hot-reload");
            }

            App.DotnetWatchArgs.Add("--launch-profile");
            App.DotnetWatchArgs.Add("Second");
            App.Start(testAsset, []);
            Assert.AreEqual("<<<Second>>>", await App.AssertOutputLineStartsWith("DOTNET_LAUNCH_PROFILE = "));
        }

        [TestMethod]
        [CombinatorialData]
        public async Task RunsWithDotnetLaunchProfileEnvVariableWhenExplicitlySpecifiedButNotPresentIsEmpty(bool hotReload)
        {
            Assert.IsTrue(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE")), "DOTNET_LAUNCH_PROFILE cannot be set already when this test is running");

            var testAsset = TestAssets.CopyTestAsset(AppName, identifier: hotReload.ToString())
                .WithSource();

            if (!hotReload)
            {
                App.DotnetWatchArgs.Add("--no-hot-reload");
            }

            App.Start(testAsset, ["--", "--launch-profile", "Third"]);
            Assert.AreEqual("<<<First>>>", await App.AssertOutputLineStartsWith("DOTNET_LAUNCH_PROFILE = "));
        }

        [TestMethod]
        public async Task RunsWithIterationEnvVariable()
        {
            var testAsset = TestAssets.CopyTestAsset(AppName)
                .WithSource();

            App.Start(testAsset, []);

            await App.AssertStarted();

            var source = Path.Combine(testAsset.Path, "Program.cs");
            var contents = File.ReadAllText(source);
            const string messagePrefix = "DOTNET_WATCH_ITERATION = ";

            var value = await App.AssertOutputLineStartsWith(messagePrefix);
            Assert.AreEqual(1, int.Parse(value, CultureInfo.InvariantCulture));

            await App.AssertOutputLineStartsWith(MessageDescriptor.WaitingForFileChangeBeforeRestarting);

            UpdateSourceFile(source);
            await App.AssertStarted();

            value = await App.AssertOutputLineStartsWith(messagePrefix);
            Assert.AreEqual(2, int.Parse(value, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task Run_WithHotReloadEnabled_ReadsLaunchSettings()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchAppWithLaunchSettings")
                .WithSource();

            App.Start(testAsset, []);

            await App.AssertOutputLineEquals("Environment: Development");
        }

        [TestMethod]
        public async Task Run_WithHotReloadEnabled_ReadsLaunchSettings_WhenUsingProjectOption()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchAppWithLaunchSettings")
                .WithSource();

            var directoryInfo = new DirectoryInfo(testAsset.Path);

            // Configure the working directory to be one level above the test app directory.
            App.Start(
                testAsset,
                ["--project", Path.Combine(directoryInfo.Name, "WatchAppWithLaunchSettings.csproj")],
                workingDirectory: Path.GetFullPath(directoryInfo.Parent.FullName));

            await App.AssertOutputLineEquals("Environment: Development");
        }

        [CoreMSBuildOnlyTestMethod(Skip = "https://github.com/dotnet/sdk/issues/29047")]
        public async Task Run_WithHotReloadEnabled_DoesNotReadConsoleIn_InNonInteractiveMode()
        {
            var testAsset = TestAssets.CopyTestAsset("WatchAppWithLaunchSettings")
                .WithSource();

            App.EnvironmentVariables.Add("READ_INPUT", "true");
            App.Start(testAsset, ["--non-interactive"]);

            await App.AssertStarted();

            var standardInput = App.Process.Process.StandardInput;
            var inputString = "This is a test input";

            await standardInput.WriteLineAsync(inputString);
            Assert.AreEqual(inputString, await App.AssertOutputLineStartsWith("Echo: "));
        }
    }
}
