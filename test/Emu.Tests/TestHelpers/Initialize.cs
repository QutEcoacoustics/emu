// <copyright file="Initialize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Bogus;

    public static class Initialize
    {
        static Initialize()
        {
            GlobalSeed = DateTime.Now.Ticks;
            Random = new Random((int)GlobalSeed);
            Randomizer.Seed = Random;
        }

        public static long GlobalSeed { get; }

        public static Random Random { get; }
    }
}
