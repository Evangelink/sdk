// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace EndToEnd.Tests
{
    [TestClass]
    public class GivenUnixPlatform(MSTestContext testContext) : SdkTest(testContext)
    {
        [TestMethod][OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
        [DataRow("wpf")]
        [DataRow("winforms")]
        public void ItDoesNotIncludeWindowsOnlyProjectTemplates(string template)
        {
            var directory = _testAssetsManager.CreateTestDirectory(identifier: template);

            new DotnetNewCommand(MSTestContext)
                .WithVirtualHive()
                .WithWorkingDirectory(directory.Path)
                .Execute(template).Should().Fail()
                    .And.HaveStdErrContaining($": '{template}'.");
        }
    }
}
