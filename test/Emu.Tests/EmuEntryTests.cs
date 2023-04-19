// <copyright file="EmuEntryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests
{
    using System.CommandLine;
    using System.Threading.Tasks;
    using Emu.Tests.TestHelpers;
    using Xunit;
    using static Emu.EmuCommand;

    public class EmuEntryTests
    {
        [Fact]
        public async Task EmuFixCheckWorks()
        {
            var result = await EmuEntry.Main(
              @"fix check C:\Work\Github\metadata-utility\test\Fixtures\FL_BAR_LT\3.17_Duration\*.flac -f FL010".Split(' '));

            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData("", LogLevel.Info)]
        [InlineData("-v", LogLevel.Debug)]
        [InlineData("-vv", LogLevel.Trace)]
        [InlineData("--verbose", LogLevel.Debug)]
        [InlineData("--very-verbose", LogLevel.Trace)]
        [InlineData("-l 0", LogLevel.None)]
        [InlineData("-l 1", LogLevel.Crit)]
        [InlineData("-l 2", LogLevel.Error)]
        [InlineData("-l 3", LogLevel.Warn)]
        [InlineData("-l 4", LogLevel.Info)]
        [InlineData("-l 5", LogLevel.Debug)]
        [InlineData("-l 6", LogLevel.Trace)]
        public void ProcessArgumentsVerbosity(string arg, LogLevel expectedValue)
        {
            //var (app, mainArgs) = EmuEntry.ProcessArguments(arg.Split(' '));
            var command = new EmuCommand();
            var result = command.Parse(arg);

            Assert.Equal(expectedValue, EmuCommand.GetLogLevel(result));
        }
    }
}
