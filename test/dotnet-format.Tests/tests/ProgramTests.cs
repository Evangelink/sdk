// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#nullable disable

using Microsoft.CodeAnalysis.Tools.Commands;

namespace Microsoft.CodeAnalysis.Tools.Tests
{
    public class ProgramTests
    {
        [TestMethod]
        public void ExitCodeIsOneWithCheckAndAnyFilesFormatted()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 1, fileCount: 0, exitCode: 0);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: true);

            Assert.AreEqual(FormatCommandCommon.CheckFailedExitCode, exitCode);
        }

        [TestMethod]
        public void ExitCodeIsZeroWithCheckAndNoFilesFormatted()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 0, fileCount: 0, exitCode: 42);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: true);

            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public void ExitCodeIsSameWithoutCheck()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 0, fileCount: 0, exitCode: 42);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: false);

            Assert.AreEqual(formatResult.ExitCode, exitCode);
        }

        [TestMethod]
        public void CommandLine_OptionsAreParsedCorrectly()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] {
                "--no-restore",
                "--include", "include1", "include2",
                "--exclude", "exclude1", "exclude2",
                "--verify-no-changes",
                "--binarylog", "binary-log-path",
                "--report", "report",
                "--verbosity", "detailed",
                "--include-generated"});

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.HasCount(0, result.UnmatchedTokens);
            Assert.HasCount(0, result.UnmatchedTokens);
            result.GetValue(FormatCommandCommon.NoRestoreOption);
            Assert.Collection(result.GetValue(FormatCommandCommon.IncludeOption),
                i0 => Assert.AreEqual("include1", i0),
                i1 => Assert.AreEqual("include2", i1));
            Assert.Collection(result.GetValue(FormatCommandCommon.ExcludeOption),
                i0 => Assert.AreEqual("exclude1", i0),
                i1 => Assert.AreEqual("exclude2", i1));
            Assert.IsTrue(result.GetValue(FormatCommandCommon.VerifyNoChanges));
            Assert.AreEqual("binary-log-path", result.GetValue(FormatCommandCommon.BinarylogOption));
            Assert.AreEqual("report", result.GetValue(FormatCommandCommon.ReportOption));
            Assert.AreEqual("detailed", result.GetValue(FormatCommandCommon.VerbosityOption));
            Assert.IsTrue(result.GetValue(FormatCommandCommon.IncludeGeneratedOption));
        }

        [TestMethod]
        public void CommandLine_ProjectArgument_Simple()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue" });

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.AreEqual("workspaceValue", result.GetValue(FormatCommandCommon.SlnOrProjectArgument));
        }

        [TestMethod]
        public void CommandLine_ProjectArgument_WithOption_AfterArgument()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue", "--verbosity", "detailed" });

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.AreEqual("workspaceValue", result.GetValue(FormatCommandCommon.SlnOrProjectArgument));
            Assert.AreEqual("detailed", result.GetValue(FormatCommandCommon.VerbosityOption));
        }

        [TestMethod]
        public void CommandLine_ProjectArgument_WithOption_BeforeArgument()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--verbosity", "detailed", "workspaceValue" });

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.AreEqual("workspaceValue", result.GetValue(FormatCommandCommon.SlnOrProjectArgument));
            Assert.AreEqual("detailed", result.GetValue(FormatCommandCommon.VerbosityOption));
        }

        [TestMethod]
        public void CommandLine_ProjectArgument_FailsIfSpecifiedTwice()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue1", "workspaceValue2" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_FolderValidation_FailsIfFixAnalyzersSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--folder", "--fix-analyzers" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_FolderValidation_FailsIfFixStyleSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--folder", "--fix-style" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_FolderValidation_FailsIfNoRestoreSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "whitespace", "--folder", "--no-restore" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_BinaryLog_DoesNotFailIfPathNotSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--binarylog" });

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.IsNotNull(result.GetResult(FormatCommandCommon.BinarylogOption));
        }

        [TestMethod]
        public void CommandLine_BinaryLog_DoesNotFailIfPathIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--binarylog", "log" });

            // Assert
            Assert.HasCount(0, result.Errors);
            Assert.IsNotNull(result.GetResult(FormatCommandCommon.BinarylogOption));
        }

        [TestMethod]
        public void CommandLine_BinaryLog_FailsIfFolderIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "whitespace", "--folder", "--binarylog" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_Diagnostics_FailsIfDiagnosticNoSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics" });

            // Assert
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void CommandLine_Diagnostics_DoesNotFailIfDiagnosticIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics", "RS0016" });

            // Assert
            Assert.HasCount(0, result.Errors);
        }

        [TestMethod]
        public void CommandLine_Diagnostics_DoesNotFailIfMultipleDiagnosticAreSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics", "RS0016", "RS0017", "RS0018" });

            // Assert
            Assert.HasCount(0, result.Errors);
        }
    }
}
