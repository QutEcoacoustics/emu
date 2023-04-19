// <copyright file="Initialize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Bogus;

    using FluentAssertions;

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

        public static void ConfigureFluentAssertions()
        {
            // when compile types are respected (the default) an
            // accidental comparison between a child and parent type will only test
            // properties that are common to them!
            // Respecting runtime types essentially ensures that apples are compared to
            // apples, but at the cost of allowing more flexible equivalency options
            // (e.g. testing two disparate types for the same props and vals is disabled)
            AssertionOptions.AssertEquivalencyUsing(o => o.RespectingRuntimeTypes());
        }
    }
}
