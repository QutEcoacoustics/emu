// <copyright file="Helpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

#pragma warning disable SA1200 // Using directives should be placed correctly
global using Microsoft.Extensions.DependencyInjection;
#pragma warning restore SA1200 // Using directives should be placed correctly

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Reflection;
    using Emu.Serialization.Converters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;

    public static class Helpers
    {
        public const string SolutionFragment = "../../../../../..";
        public const string FixturesFragment = "test/Fixtures";

        /// <summary>
        /// About 111.32mm at the equator
        /// https://en.wikipedia.org/wiki/Decimal_degrees.
        /// </summary>
        public const int Wgs84Epsilon = 6;

        public static readonly Action Nop = () => { };

        private static readonly NullLoggerFactory NullLoggerFactory = new();

        private static JsonSerializerSettings jsonSettings;

        public static JsonSerializerSettings JsonSettings
        {
            get
            {
                return jsonSettings ??= new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter(),
                        new WellKnownProblemJsonConverter(),
                        new JsonRangeConverter(),
                        new RationalNullJsonConverter(serializeAsString: true),
                        new RationalJsonConverter(serializeAsString: true),
                    },
                }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            }
        }

        public static IFileSystem RealFileSystem { get; } = new FileSystem();

        public static string SolutionRoot => RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Assembly.GetExecutingAssembly().Location, SolutionFragment));

        public static string FixturesRoot => RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(SolutionRoot, FixturesFragment));

        public static string TestTempRoot => RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(SolutionRoot, "temp"));

        public static ILogger<T> NullLogger<T>()
        {
            return new Logger<T>(NullLoggerFactory);
        }
    }
}
