// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework
{
    public class FullMSBuildConditionAttribute : ConditionBaseAttribute
    {
        public FullMSBuildConditionAttribute() : base(ConditionMode.Include)
        {
        }

        public override string? IgnoreMessage { get; } = "This test requires Full MSBuild to run";

        public override string GroupName { get; } = "MSBuild";

        public override bool ShouldRun => TestContext.Current.ToolsetUnderTest.ShouldUseFullFrameworkMSBuild;
    }
}
