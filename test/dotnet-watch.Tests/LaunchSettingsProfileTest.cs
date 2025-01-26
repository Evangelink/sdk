// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Watch.UnitTests;

[TestClass]
public class LaunchSettingsProfileTest
{
    private readonly IReporter _reporter;
    private readonly TestAssetsManager _testAssets;

    public LaunchSettingsProfileTest(MSTestContext testContext)
    {
        _reporter = new TestReporter(testContext);
        _testAssets = new TestAssetsManager(testContext);
    }

    [TestMethod]
    public void LoadsLaunchProfiles()
    {
        var project = _testAssets.CreateTestProject(new TestProject("Project1")
        {
            TargetFrameworks = ToolsetInfo.CurrentTargetFramework,
        });

        WriteFile(project, Path.Combine("Properties", "launchSettings.json"),
        """
        {
          "profiles": {
            "http": {
              "applicationUrl": "http://localhost:5000",
              "commandName": "Project",
              "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
              }
            },
            "https": {
              "applicationUrl": "https://localhost:5001",
              "commandName": "Project",
              "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
              }
            }, // This comment and trailing comma shouldn't cause any issues
          }
        }
        """);

        var projectDirectory = Path.Combine(project.TestRoot, "Project1");

        var expected = LaunchSettingsProfile.ReadLaunchProfile(projectDirectory, "http", _reporter);
        Assert.IsNotNull(expected);
        Assert.AreEqual("http://localhost:5000", expected.ApplicationUrl);

        expected = LaunchSettingsProfile.ReadLaunchProfile(projectDirectory, "https", _reporter);
        Assert.IsNotNull(expected);
        Assert.AreEqual("https://localhost:5001", expected.ApplicationUrl);

        expected = LaunchSettingsProfile.ReadLaunchProfile(projectDirectory, "notfound", _reporter);
        Assert.IsNotNull(expected);
    }

    private static string WriteFile(TestAsset testAsset, string name, string contents = "")
    {
        var path = Path.Combine(GetTestProjectDirectory(testAsset), name);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, contents);

        return path;
    }

    private static string WriteFile(TestDirectory testAsset, string name, string contents = "")
    {
        var path = Path.Combine(testAsset.Path, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, contents);

        return path;
    }

    private static string GetTestProjectDirectory(TestAsset testAsset)
        => Path.Combine(testAsset.Path, testAsset.TestProject.Name);
}
