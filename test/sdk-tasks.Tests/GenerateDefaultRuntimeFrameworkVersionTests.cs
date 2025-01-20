using Microsoft.DotNet.Build.Tasks;

namespace Microsoft.CoreSdkTasks.Tests
{
    public class GenerateDefaultRuntimeFrameworkVersionTests(TestContext testContext) : SdkTest(log)
    {
        [TestMethod]
        [DataRow("3.0.0-rtm", "3.0.0-rtm")]
        [DataRow("3.1.0", "3.1.0")]
        [DataRow("10.3.10", "10.3.0")]
        [DataRow("1.1.10-prerelease", "1.1.0")]
        public void ItGeneratesDefaultVersionBasedOnRuntimePackVersion(string runtimePackVersion, string defaultRuntimeFrameworkVersion)
        {
            var generateTask = new GenerateDefaultRuntimeFrameworkVersion()
            {
                RuntimePackVersion = runtimePackVersion
            };

            generateTask
                .Execute()
                .Should().BeTrue();

            generateTask.DefaultRuntimeFrameworkVersion.Should().Be(defaultRuntimeFrameworkVersion);
        }
    }
}
