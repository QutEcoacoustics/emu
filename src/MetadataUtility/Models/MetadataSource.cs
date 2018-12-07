// <copyright file="MetadataSource.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Allows us to track the provenance of metadata.
    /// </summary>
    /// <typeparam name="T">The type of value we are storing.</typeparam>
    public struct MetadataSource<T>
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
        /// The enum representing the provenance of a metadata value.
        /// </summary>
        public enum Provenance
        {
            /// <summary>
            /// We don't know where this metadata came from.
            /// </summary>
            Unknown,

            /// <summary>
            /// This metadata was extracted from the filename.
            /// </summary>
            Filename,

            /// <summary>
            /// This metadata was extracted from the log file found near the file.
            /// </summary>
            LogFile,

            /// <summary>
            /// This metadata was extracted from some file found near the file.
            /// </summary>
            OtherFile,

            /// <summary>
            /// This metadata was extracted from the header of the file.
            /// </summary>
            FileHeader,

            /// <summary>
            /// This value was calculated from other metadata values.
            /// </summary>
            Calculated,
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

    public static class MetadataSourceExtensions
    {
        public static MetadataSource<T> Wrap<T>(this MetadataSource<T>.Provenance provenance, T value)
        {
            return new MetadataSource<T>(value, provenance);
        }
    }
}
