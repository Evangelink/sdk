// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.NET.Build.Containers.IntegrationTests;

static class ContainerCli
{
    public static bool IsPodman => _isPodman.Value;

    public static bool IsAvailable => _isAvailable.Value;

    public static RunExeCommand PullCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "pull", args);

    public static RunExeCommand TagCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "tag", args);

    public static RunExeCommand PushCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "push", args);

    public static RunExeCommand StopCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "stop", args);

    public static RunExeCommand RunCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "run", args);

    public static RunExeCommand LogsCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "logs", args);

    public static RunExeCommand LoginCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "login", args);

    public static RunExeCommand InspectCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "inspect", args);

    public static RunExeCommand LoadCommand(MSTestContext testContext, params string[] args)
      => CreateCommand(testContext, "load", args);

    public static RunExeCommand PortCommand(MSTestContext testContext, string containerName, int port)
      => CreateCommand(testContext, "port", containerName, port.ToString());

    private static RunExeCommand CreateCommand(MSTestContext testContext, string command, params string[] args)
    {
        string commandPath = IsPodman ? "podman" : "docker";

        // The local registry is not accessible via https.
        // Podman doesn't want to use it unless we set 'tls-verify' to 'false'.
        if (IsPodman && (command == "push" || command == "pull" || command == "login"))
        {
            if (args.Length > 0)
            {
                string image = args[args.Length - 1];
                if (image.StartsWith($"localhost:"))
                {
                    args = new[] { "--tls-verify=false" }.Concat(args).ToArray();
                }
            }
        }

        return new RunExeCommand(testContext, commandPath, new[] { command }.Concat(args).ToArray());
    }

    private static readonly Lazy<bool> _isPodman =
      new(() => new DockerCli(loggerFactory: new TestLoggerFactory()).GetCommand() == DockerCli.PodmanCommand);

    private static readonly Lazy<bool> _isAvailable =
      new(() => new DockerCli(loggerFactory: new TestLoggerFactory()).IsAvailable());
}
