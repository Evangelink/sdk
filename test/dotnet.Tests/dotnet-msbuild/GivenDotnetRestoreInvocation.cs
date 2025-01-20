// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using RestoreCommand = Microsoft.DotNet.Tools.Restore.RestoreCommand;

namespace Microsoft.DotNet.Cli.MSBuild.Tests
{
    [Collection(TestConstants.UsesStaticTelemetryState)]
    public class GivenDotnetRestoreInvocation : IClassFixture<NullCurrentSessionIdFixture>
    {
        private const string ExpectedPrefix =
            "-maxcpucount -verbosity:m -tlp:default=auto -nologo -target:Restore";
        private static readonly string WorkingDirectory =
            TestPathUtilities.FormatAbsolutePath(nameof(GivenDotnetRestoreInvocation));

        [TestMethod]
        [DataRow(new string[] { }, "")]
        [DataRow(new string[] { "-s", "<source>" }, "-property:RestoreSources=<source>")]
        [DataRow(new string[] { "--source", "<source>" }, "-property:RestoreSources=<source>")]
        [DataRow(new string[] { "-s", "<source0>", "-s", "<source1>" }, "-property:RestoreSources=<source0>%3B<source1>")]
        [DataRow(new string[] { "-r", "<runtime>" }, "-property:RuntimeIdentifiers=<runtime>")]
        [DataRow(new string[] { "-r", "linux-amd64" }, "-property:RuntimeIdentifiers=linux-x64")]
        [DataRow(new string[] { "--runtime", "<runtime>" }, "-property:RuntimeIdentifiers=<runtime>")]
        [DataRow(new string[] { "-r", "<runtime0>", "-r", "<runtime1>" }, "-property:RuntimeIdentifiers=<runtime0>%3B<runtime1>")]
        [DataRow(new string[] { "--packages", "<packages>" }, "-property:RestorePackagesPath=<cwd><packages>")]
        [DataRow(new string[] { "--disable-parallel" }, "-property:RestoreDisableParallel=true")]
        [DataRow(new string[] { "--configfile", "<config>" }, "-property:RestoreConfigFile=<cwd><config>")]
        [DataRow(new string[] { "--no-cache" }, "-property:RestoreNoCache=true")]
        [DataRow(new string[] { "--no-http-cache" }, "-property:RestoreNoHttpCache=true")]
        [DataRow(new string[] { "--ignore-failed-sources" }, "-property:RestoreIgnoreFailedSources=true")]
        [DataRow(new string[] { "--no-dependencies" }, "-property:RestoreRecursive=false")]
        [DataRow(new string[] { "-v", "minimal" }, @"-verbosity:minimal")]
        [DataRow(new string[] { "--verbosity", "minimal" }, @"-verbosity:minimal")]
        [DataRow(new string[] { "--use-lock-file" }, "-property:RestorePackagesWithLockFile=true")]
        [DataRow(new string[] { "--locked-mode" }, "-property:RestoreLockedMode=true")]
        [DataRow(new string[] { "--force-evaluate" }, "-property:RestoreForceEvaluate=true")]
        [DataRow(new string[] { "--lock-file-path", "<lockFilePath>" }, "-property:NuGetLockFilePath=<lockFilePath>")]
        [DataRow(new string[] { "--disable-build-servers" }, "--property:UseRazorBuildServer=false --property:UseSharedCompilation=false /nodeReuse:false")]
        public void MsbuildInvocationIsCorrect(string[] args, string expectedAdditionalArgs)
        {
            CommandDirectoryContext.PerformActionWithBasePath(WorkingDirectory, () =>
            {
                Telemetry.Telemetry.DisableForTests();

                expectedAdditionalArgs =
                    (string.IsNullOrEmpty(expectedAdditionalArgs) ? "" : $" {expectedAdditionalArgs}")
                    .Replace("<cwd>", WorkingDirectory);

                var msbuildPath = "<msbuildpath>";
                RestoreCommand.FromArgs(args, msbuildPath)
                    .GetArgumentsToMSBuild()
                    .Should().Be($"{ExpectedPrefix}{expectedAdditionalArgs}");
            });
        }
    }
}
