// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.ToolPackage;
using Microsoft.DotNet.Tools;

namespace Microsoft.DotNet.PackageInstall.Tests
{
    public class ToolConfigurationDeserializerTests
    {
        [TestMethod]
        public void GivenXmlPathItShouldGetToolConfiguration()
        {
            ToolConfiguration toolConfiguration = ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsGolden.xml");

            toolConfiguration.CommandName.Should().Be("sayhello");
            toolConfiguration.ToolAssemblyEntryPoint.Should().Be("console.dll");
        }

        [TestMethod]
        public void GivenMalformedPathItThrows()
        {
            Action a = () => ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsMalformed.xml");
            a.Should().Throw<ToolConfigurationException>()
                .And.Message.Should()
                .Contain(string.Format(CommonLocalizableStrings.ToolSettingsInvalidXml, string.Empty));
        }

        [TestMethod]
        public void GivenMissingContentItThrows()
        {
            Action a = () => ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsMissing.xml");
            a.Should().Throw<ToolConfigurationException>()
                .And.Message.Should()
                .Contain(CommonLocalizableStrings.ToolSettingsMissingCommandName);
        }

        [TestMethod]
        public void GivenMissingVersionItHasWarningReflectIt()
        {
            ToolConfiguration toolConfiguration = ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsMissingVersion.xml");

            toolConfiguration.Warnings.First().Should().Be(CommonLocalizableStrings.FormatVersionIsMissing);
        }

        [TestMethod]
        public void GivenMajorHigherVersionItHasWarningReflectIt()
        {
            ToolConfiguration toolConfiguration = ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsMajorHigherVersion.xml");

            toolConfiguration.Warnings.First().Should().Be(CommonLocalizableStrings.FormatVersionIsHigher);
        }

        [TestMethod]
        public void GivenMinorHigherVersionItHasNoWarning()
        {
            ToolConfiguration toolConfiguration = ToolConfigurationDeserializer.Deserialize("DotnetToolSettingsGolden.xml");

            toolConfiguration.Warnings.Should().BeEmpty();
        }

        [TestMethod]
        public void GivenInvalidCharAsFileNameItThrows()
        {
            var invalidCommandName = "na\0me";
            Action a = () => new ToolConfiguration(invalidCommandName, "my.dll");
            a.Should().Throw<ToolConfigurationException>()
                .And.Message.Should()
                .Contain(
                    string.Format(
                        CommonLocalizableStrings.ToolSettingsInvalidCommandName,
                        invalidCommandName,
                        string.Join(", ", Path.GetInvalidFileNameChars().Select(c => $"'{c}'"))));
        }

        [TestMethod]
        public void GivenALeadingDotAsFileNameItThrows()
        {
            var invalidCommandName = ".mytool";
            Action a = () => new ToolConfiguration(invalidCommandName, "my.dll");
            a.Should().Throw<ToolConfigurationException>()
                .And.Message.Should()
                .Contain(string.Format(
                        CommonLocalizableStrings.ToolSettingsInvalidLeadingDotCommandName,
                        invalidCommandName));
        }
    }
}
