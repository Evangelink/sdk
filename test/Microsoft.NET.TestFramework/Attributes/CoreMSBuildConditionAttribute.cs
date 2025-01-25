// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.TestFramework
{
    public sealed class CoreMSBuildConditionAttribute : ConditionBaseAttribute
    {
        public CoreMSBuildConditionAttribute()
            : base(ConditionMode.Include)
        {
        }

        public override string? IgnoreMessage { get; } = "This test requires Core MSBuild to run";

        public override string GroupName => "MSBuild";

        public override bool ShouldRun => !TestContext.Current.ToolsetUnderTest.ShouldUseFullFrameworkMSBuild;
    }
}
