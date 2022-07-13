// <copyright file="MultiStreamWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

//  Copyright 2014, Desert Software Solutions Inc.
//    MultiStreamWriter.cs: https://gist.github.com/rostreim/9441193
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace Emu.Tests.TestHelpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MultiStreamWriter : TextWriter
{
    private Encoding encoding = Encoding.Default;
    private IFormatProvider formatProvider = null;
    private List<TextWriter> writers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiStreamWriter"/> class.
    /// </summary>
    /// <param name="writers">The writers.</param>
    public MultiStreamWriter(params TextWriter[] writers)
    {
        this.writers.AddRange(writers);
    }

    /// <summary>
    /// Gets When overridden in a derived class, returns the character encoding in which the output is written.
    /// </summary>
    /// <returns>The character encoding in which the output is written.</returns>
    public override Encoding Encoding
    {
        get { return this.encoding; }
    }

    /// <summary>
    /// Gets or sets the line terminator string used by the current TextWriter.
    /// </summary>
    /// <returns>The line terminator string for the current TextWriter.</returns>
    public override string NewLine
    {
        get => base.NewLine;

        set
        {
            foreach (var writer in this.writers)
            {
                writer.NewLine = value;
            }

            base.NewLine = value;
        }
    }

    /// <summary>
    /// Gets an object that controls formatting.
    /// </summary>
    /// <returns>An <see cref="T:System.IFormatProvider" /> object for a specific culture, or the formatting of the current culture if no other culture is specified.</returns>
    public override IFormatProvider FormatProvider
    {
        get { return this.formatProvider ?? base.FormatProvider; }
    }

    /// <summary>
    /// Adds one or more writers.
    /// </summary>
    /// <param name="writer">The writers to be added.</param>
    public MultiStreamWriter AddWriter(params TextWriter[] writer)
    {
        this.writers.AddRange(writer);
        return this;
    }

    /// <summary>
    /// Adds one or more writers.
    /// </summary>
    /// <param name="writer">The streams to be added.</param>
    public MultiStreamWriter AddWriter(params Stream[] writer)
    {
        foreach (var stream in writer)
        {
            this.writers.Add(new StreamWriter(stream));
        }

        return this;
    }

    /// <summary>
    /// Removes all of the writers.
    /// </summary>
    public MultiStreamWriter Clear()
    {
        this.writers.Clear();
        return this;
    }

    /// <summary>
    /// Sets the encoding.
    /// </summary>
    /// <param name="value">The value.</param>
    public MultiStreamWriter SetEncoding(System.Text.Encoding value)
    {
        this.encoding = value;
        return this;
    }

    /// <summary>
    /// Closes the current writer and releases any system resources associated with the writer.
    /// </summary>
    public override void Close()
    {
        foreach (var writer in this.writers)
        {
            writer.Close();
        }

        base.Close();
    }

    /// <summary>
    /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
    /// </summary>
    public override void Flush()
    {
        foreach (var writer in this.writers)
        {
            writer.Flush();
        }

        base.Flush();
    }

    /// <summary>
    /// Writes the text representation of a Boolean value to the text string or stream.
    /// </summary>
    /// <param name="value">The Boolean value to write.</param>
    public override void Write(bool value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes a character to the text string or stream.
    /// </summary>
    /// <param name="value">The character to write to the text stream.</param>
    public override void Write(char value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes a character array to the text string or stream.
    /// </summary>
    /// <param name="buffer">The character array to write to the text stream.</param>
    public override void Write(char[] buffer)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(buffer);
        }
    }

    /// <summary>
    /// Writes the text representation of a decimal value to the text string or stream.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    public override void Write(decimal value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an 8-byte floating-point value to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte floating-point value to write.</param>
    public override void Write(double value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte floating-point value to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte floating-point value to write.</param>
    public override void Write(float value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte signed integer to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte signed integer to write.</param>
    public override void Write(int value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an 8-byte signed integer to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte signed integer to write.</param>
    public override void Write(long value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an object to the text string or stream by calling the ToString method on that object.
    /// </summary>
    /// <param name="value">The object to write.</param>
    public override void Write(object value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes a string to the text string or stream.
    /// </summary>
    /// <param name="value">The string to write.</param>
    public override void Write(string value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte unsigned integer to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte unsigned integer to write.</param>
    public override void Write(uint value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an 8-byte unsigned integer to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte unsigned integer to write.</param>
    public override void Write(ulong value)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The object to format and write.</param>
    public override void Write(string format, object arg0)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(format, arg0);
        }
    }

    /// <summary>
    /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object[])" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
    public override void Write(string format, params object[] arg)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(format, arg);
        }
    }

    /// <summary>
    /// Writes a subarray of characters to the text string or stream.
    /// </summary>
    /// <param name="buffer">The character array to write data from.</param>
    /// <param name="index">The character position in the buffer at which to start retrieving data.</param>
    /// <param name="count">The number of characters to write.</param>
    public override void Write(char[] buffer, int index, int count)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(buffer, index, count);
        }
    }

    /// <summary>
    /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The first object to format and write.</param>
    /// <param name="arg1">The second object to format and write.</param>
    public override void Write(string format, object arg0, object arg1)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(format, arg0, arg1);
        }
    }

    /// <summary>
    /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object,System.Object)" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The first object to format and write.</param>
    /// <param name="arg1">The second object to format and write.</param>
    /// <param name="arg2">The third object to format and write.</param>
    public override void Write(string format, object arg0, object arg1, object arg2)
    {
        foreach (var writer in this.writers)
        {
            writer.Write(format, arg0, arg1, arg2);
        }
    }

    /// <summary>
    /// Writes a line terminator to the text string or stream.
    /// </summary>
    public override void WriteLine()
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine();
        }
    }

    /// <summary>
    /// Writes the text representation of a Boolean value followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The Boolean value to write.</param>
    public override void WriteLine(bool value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes a character followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The character to write to the text stream.</param>
    public override void WriteLine(char value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes an array of characters followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="buffer">The character array from which data is read.</param>
    public override void WriteLine(char[] buffer)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(buffer);
        }
    }

    /// <summary>
    /// Writes the text representation of a decimal value followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    public override void WriteLine(decimal value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 8-byte floating-point value followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte floating-point value to write.</param>
    public override void WriteLine(double value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte floating-point value followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte floating-point value to write.</param>
    public override void WriteLine(float value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte signed integer followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte signed integer to write.</param>
    public override void WriteLine(int value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an 8-byte signed integer followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte signed integer to write.</param>
    public override void WriteLine(long value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an object by calling the ToString method on that object, followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The object to write. If <paramref name="value" /> is null, only the line terminator is written.</param>
    public override void WriteLine(object value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes a string followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The string to write. If <paramref name="value" /> is null, only the line terminator is written.</param>
    public override void WriteLine(string value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of a 4-byte unsigned integer followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 4-byte unsigned integer to write.</param>
    public override void WriteLine(uint value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes the text representation of an 8-byte unsigned integer followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="value">The 8-byte unsigned integer to write.</param>
    public override void WriteLine(ulong value)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(value);
        }
    }

    /// <summary>
    /// Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The object to format and write.</param>
    public override void WriteLine(string format, object arg0)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(format, arg0);
        }
    }

    /// <summary>
    /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)" />.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
    public override void WriteLine(string format, params object[] arg)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(format, arg);
        }
    }

    /// <summary>
    /// Writes a subarray of characters followed by a line terminator to the text string or stream.
    /// </summary>
    /// <param name="buffer">The character array from which data is read.</param>
    /// <param name="index">The character position in <paramref name="buffer" /> at which to start reading data.</param>
    /// <param name="count">The maximum number of characters to write.</param>
    public override void WriteLine(char[] buffer, int index, int count)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(buffer, index, count);
        }
    }

    /// <summary>
    /// Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)" /> method.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The first object to format and write.</param>
    /// <param name="arg1">The second object to format and write.</param>
    public override void WriteLine(string format, object arg0, object arg1)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(format, arg0, arg1);
        }
    }

    /// <summary>
    /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)" />.
    /// </summary>
    /// <param name="format">A composite format string (see Remarks).</param>
    /// <param name="arg0">The first object to format and write.</param>
    /// <param name="arg1">The second object to format and write.</param>
    /// <param name="arg2">The third object to format and write.</param>
    public override void WriteLine(string format, object arg0, object arg1, object arg2)
    {
        foreach (var writer in this.writers)
        {
            writer.WriteLine(format, arg0, arg1, arg2);
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.IO.TextWriter" /> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var writer in this.writers)
            {
                writer.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
