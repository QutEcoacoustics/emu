// <copyright file="GlobalSuppressions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>
// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("System.IO.Abstractions", "IO0008:Replace StringWriter class with IFileSystem.StringWriter for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~N:MetadataUtility")]
[assembly: SuppressMessage("System.IO.Abstractions", "IO0008:Replace StringWriter class with IFileSystem.StringWriter for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~N:MetadataUtility")]
[assembly: SuppressMessage("System.IO.Abstractions", "IO0009:Replace StringReader instances with IFileSystem.StringReader factory for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~M:MetadataUtility.Serialization.JsonLinesSerializer.Deserialize``1(System.IO.TextReader)~System.Collections.Generic.IEnumerable{``0}")]
[assembly: SuppressMessage("System.IO.Abstractions", "IO0008:Replace StringWriter class with IFileSystem.StringWriter for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~M:MetadataUtility.Serialization.CsvSerializer.Serialize``1(System.Collections.Generic.IEnumerable{``0})~System.String")]
[assembly: SuppressMessage("System.IO.Abstractions", "IO0008:Replace StringWriter class with IFileSystem.StringWriter for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~M:MetadataUtility.Serialization.JsonLinesSerializer.Serialize``1(System.Collections.Generic.IEnumerable{``0})~System.String")]
[assembly: SuppressMessage("System.IO.Abstractions", "IO0008:Replace StringWriter class with IFileSystem.StringWriter for improved testability", Justification = "Seems like a false positive", Scope = "member", Target = "~M:MetadataUtility.Serialization.JsonSerializer.Serialize``1(System.Collections.Generic.IEnumerable{``0})~System.String")]
