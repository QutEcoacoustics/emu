// <copyright file="MetadataSource.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Allows us to track the provenance of metadata.
    /// </summary>
    /// <typeparam name="T">The type of value we are storing.</typeparam>
    public readonly struct MetadataSource<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSource{T}"/> struct.
        /// </summary>
        /// <param name="value">The value we are wrapping.</param>
        /// <param name="source">The provenance of the <paramref name="value"/>.</param>
        public MetadataSource(T value, Provenance source)
        {
            this.Value = value;
            this.Source = source;
        }

        /// <summary>
        /// Gets the metadata value we are wrapping.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the provenance information for <see cref="Value"/>.
        /// </summary>
        public Provenance Source { get; }

        /// <summary>
        /// Extracts the value from a <see cref="MetadataSource{T}"/>.
        /// </summary>
        /// <param name="metadataSource">The value to operate on.</param>
        public static implicit operator T(MetadataSource<T> metadataSource)
        {
            return metadataSource.Value;
        }
    }

    /// <summary>
    /// Extensions to <see cref="MetadataSource{T}"/>.
    /// </summary>
    public static class MetadataSourceExtensions
    {
        /// <summary>
        /// Wrap a given value in provenance information.
        /// </summary>
        /// <typeparam name="T">The type of metadata.</typeparam>
        /// <param name="provenance">Which provenance tag to attach.</param>
        /// <param name="value">The value to wrap.</param>
        /// <returns>A value tagged with provenance information.</returns>
        public static MetadataSource<T> Wrap<T>(this Provenance provenance, T value)
        {
            return new MetadataSource<T>(value, provenance);
        }

        /// <summary>
        /// Wrap a given value in provenance information.
        /// </summary>
        /// <typeparam name="T">The type of metadata.</typeparam>
        /// <param name="value">The value to attach provenance information too.</param>
        /// <param name="provenance">Which provenance tag to attach.</param>
        /// <returns>A value tagged with provenance information.</returns>
        public static MetadataSource<T> SourcedFrom<T>(this T value, Provenance provenance)
        {
            return new MetadataSource<T>(value, provenance);
        }
    }
}
