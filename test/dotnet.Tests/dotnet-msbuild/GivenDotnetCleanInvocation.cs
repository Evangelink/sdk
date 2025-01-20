// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanCommand = Microsoft.DotNet.Tools.Clean.CleanCommand;

namespace Microsoft.DotNet.Cli.MSBuild.Tests
{
    [Collection(TestConstants.UsesStaticTelemetryState)]
    public class GivenDotnetCleanInvocation : IClassFixture<NullCurrentSessionIdFixture>
    {
        const string ExpectedPrefix = "-maxcpucount -verbosity:m -tlp:default=auto -nologo -verbosity:normal -target:Clean";

        private static readonly string WorkingDirectory =
            TestPathUtilities.FormatAbsolutePath(nameof(GivenDotnetCleanInvocation));

        [TestMethod]
        public void ItAddsProjectToMsbuildInvocation()
        {
            var msbuildPath = "<msbuildpath>";
            CleanCommand.FromArgs(new string[] { "<project>" }, msbuildPath)
                .GetArgumentsToMSBuild().Should().Be("-maxcpucount -verbosity:m -tlp:default=auto -nologo -verbosity:normal <project> -target:Clean");
        }

        [TestMethod]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "-o", "<output>" }, "-property:OutputPath=<cwd><output> -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--output", "<output>" }, "-property:OutputPath=<cwd><output> -property:_CommandLineDefinedOutputPath=true")]
        [DataRow(new string[] { "--artifacts-path", "foo" }, "-property:ArtifactsPath=<cwd>foo")]
        [DataRow(new string[] { "-f", "<framework>" }, "-property:TargetFramework=<framework>")]
        [DataRow(new string[] { "--framework", "<framework>" }, "-property:TargetFramework=<framework>")]
        [DataRow(new string[] { "-c", "<configuration>" }, "-property:Configuration=<configuration>")]
        [DataRow(new string[] { "--configuration", "<configuration>" }, "-property:Configuration=<configuration>")]
        [DataRow(new string[] { "-v", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "--verbosity", "diag" }, "-verbosity:diag")]
        [DataRow(new string[] { "--disable-build-servers" }, "--property:UseRazorBuildServer=false --property:UseSharedCompilation=false /nodeReuse:false")]

        public void MsbuildInvocationIsCorrect(string[] args, string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                expectedAdditionalArgs =
                    (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}")
                    .Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                CleanCommand.FromArgs(args, msbuildPath)
                    .GetArgumentsToMSBuild().Should().Be($"{ExpectedPrefix}{expectedAdditionalArgs}");
            });
        }
    }
}
