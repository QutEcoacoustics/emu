// <copyright file="MyXunitFramework.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

[assembly: Xunit.TestFramework("Emu.Tests.TestHelpers.MyXunitFramework", "Emu.Tests")]

namespace Emu.Tests.TestHelpers
{
    using Xunit.Abstractions;
    using Xunit.Sdk;

    // allow running code when xunit starts
    // https://fluentassertions.com/tips/#xunitnet
    public class MyXunitFramework : XunitTestFramework
        {
            public MyXunitFramework(IMessageSink messageSink)
                : base(messageSink)
            {
                Initialize.ConfigureFluentAssertions();
            }
        }
}
