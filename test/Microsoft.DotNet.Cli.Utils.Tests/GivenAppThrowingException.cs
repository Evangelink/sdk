// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.Utils.Tests
{
    public class GivenAppThrowingException : SdkTest
    {
        public GivenAppThrowingException(MSTestContext testContext) : base(testContext)
        {
        }

        [RequiresSpecificFrameworkTestMethod("netcoreapp1.1")]
        public void ItShowsStackTraceWhenRun()
        {
            var root = _testAssetsManager.CopyTestAsset("AppThrowingException", testAssetSubdirectory: TestAssetSubdirectories.NonRestoredTestProjects)
                .WithSource()
                .Path;

            var appRoot = Path.Combine(root, "App");

            string msg1 = "Unhandled exception. AppThrowing.MyException: "
                + "Exception of type 'AppThrowing.MyException' was thrown.";
            string msg2 = "at AppThrowing.MyException.Main(String[] args)";
            new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(appRoot)
                .Execute("run")
                .Should().Fail()
                         .And.HaveStdErrContaining(msg1)
                         .And.HaveStdErrContaining(msg2);
        }

        [RequiresSpecificFrameworkTestMethod("netcoreapp1.1")]
        public void ItShowsStackTraceWhenRunAsTool()
        {
            var root = _testAssetsManager.CopyTestAsset("AppThrowingException", testAssetSubdirectory: TestAssetSubdirectories.NonRestoredTestProjects)
                .WithSource()
                .Path;

            var appRoot = Path.Combine(root, "App");

            new DotnetPackCommand(MSTestContext)
                .WithWorkingDirectory(appRoot)
                .Execute("-o", "../pkgs")
                .Should()
                .Pass();

            var appWithToolDepRoot = Path.Combine(root, "AppDependingOnOtherAsTool");

            new RestoreCommand(MSTestContext, appWithToolDepRoot)
                .Execute()
                .Should().Pass();

            string msg1 = "Unhandled exception. AppThrowing.MyException: "
                + "Exception of type 'AppThrowing.MyException' was thrown.";
            string msg2 = "at AppThrowing.MyException.Main(String[] args)";
            new DotnetCommand(MSTestContext)
                .WithWorkingDirectory(appWithToolDepRoot)
                .Execute("throwingtool")
                .Should().Fail()
                         .And.HaveStdErrContaining(msg1)
                         .And.HaveStdErrContaining(msg2);
        }
    }
}
