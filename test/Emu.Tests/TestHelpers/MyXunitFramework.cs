// <copyright file="MyXunitFramework.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.Runtime.CompilerServices;

    // allow running code when xunit starts
    // https://fluentassertions.com/tips/#xunitnet
    public static class TestFrameworkInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            Initialize.ConfigureFluentAssertions();
        }
    }
}
