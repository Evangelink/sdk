// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Aspire.Tools.Service.UnitTests;

public class RunSessionRequestTests
{
    [TestMethod]
    public void RunSessionRequest_ToProjectLaunchRequest()
    {
        var runSessionReq = new RunSessionRequest()
        {
            Arguments = new string[] { "--someArg" },
            Environment = new EnvVar[]
             {
                new EnvVar { Name = "var1", Value = "value1"},
                new EnvVar { Name = "var2", Value = "value2"},
             },
            LaunchConfigurations = new LaunchConfiguration[]
            {
                new() {
                    ProjectPath = @"c:\test\Projects\project1.csproj",
                    LaunchType = RunSessionRequest.ProjectLaunchConfigurationType,
                    LaunchMode= RunSessionRequest.DebugLaunchMode,
                    LaunchProfile = "specificProfileName",
                    DisableLaunchProfile = true
                }
            }
        };

        var projectReq = runSessionReq.ToProjectLaunchInformation();

        Assert.AreEqual(runSessionReq.Arguments[0], projectReq.Arguments.First());
        Assert.AreEqual(runSessionReq.Environment.Length, projectReq.Environment.Count());
        Assert.AreEqual(runSessionReq.Environment[0].Name, projectReq.Environment.First().Key);
        Assert.AreEqual(runSessionReq.Environment[0].Value, projectReq.Environment.First().Value);
        Assert.AreEqual(runSessionReq.LaunchConfigurations[0].ProjectPath, projectReq.ProjectPath);
        Assert.IsTrue(projectReq.Debug);
        Assert.AreEqual(runSessionReq.LaunchConfigurations[0].LaunchProfile, projectReq.LaunchProfile);
        Assert.AreEqual(runSessionReq.LaunchConfigurations[0].DisableLaunchProfile, projectReq.DisableLaunchProfile);
    }
}
