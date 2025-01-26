// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.NET.TestFramework
{
    public class FrameworkVersionConditionAttribute : ConditionBaseAttribute
    {
        private readonly string _framework;

        public FrameworkVersionConditionAttribute(string framework)
            : base(ConditionMode.Include)
        {
            _framework = framework;
            IgnoreMessage = $"This test requires a shared framework that isn't present: {framework}";
        }

        public override string? IgnoreMessage { get; }

        public override string GroupName => "FrameworkVersion";

        public override bool ShouldRun => EnvironmentInfo.SupportsTargetFramework(_framework);
    }
}

#endif
