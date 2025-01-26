// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework
{
    public class MSBuildVersionConditionAttribute : ConditionBaseAttribute
    {
        private readonly string _version;
        private string? _ignoreMessage;

        public MSBuildVersionConditionAttribute(string version)
            : base(ConditionMode.Include)
        {
            _version = version;
        }

        /// <summary>
        /// Can be used to document the reason a test needs a specific version of MSBuild
        /// </summary>
        public override string? IgnoreMessage => _ignoreMessage;

        public string? Reason { get; set; }

        public override string GroupName { get; } = "MSBuildVersion";

        public override bool ShouldRun
        {
            get
            {
                if (!Version.TryParse(TestContext.Current.ToolsetUnderTest.MSBuildVersion, out Version? msbuildVersion))
                {
                    _ignoreMessage = $"Failed to determine the version of MSBuild ({TestContext.Current.ToolsetUnderTest.MSBuildVersion}).";
                    return false;
                }
                if (!Version.TryParse(_version, out Version? requiredVersion))
                {
                    _ignoreMessage = $"Failed to determine the version required by this test ({_version}).";
                    return false;
                }
                if (requiredVersion > msbuildVersion)
                {
                    _ignoreMessage = $"This test requires MSBuild version {_version} to run (using {TestContext.Current.ToolsetUnderTest.MSBuildVersion}).";
                    return false;
                }

                return true;
            }
        }
    }
}
