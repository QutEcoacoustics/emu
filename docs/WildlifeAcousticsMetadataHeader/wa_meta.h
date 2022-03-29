//////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2014-2017 Wildlife Acoustics, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// Wildlife Acoustics, Inc.
// 3 Mill and Main Place, Suite 210
// Maynard, MA 01754-2657
// +1 978 369 5225
// www.wildlifeacoustics.com
//
//////////////////////////////////////////////////////////////////////////////

#ifndef _WA_META_H
#define _WA_META_H

// wa_meta.h
//
// This is a header file and comments to describe the format of meta data
// found in .WAV files produced by Wildlife Acoustics Song Meter and Echo Meter
// products and used by our Kaleidoscope software.
//
// Standard .WAV files begin with the RIFF/WAVE header followed by two or
// more "chunks" including at least one "fmt " chunk and one "data" chunk.
// Wildlife Acoustics defines a new "wamd" chunk to encapsulate a number
// of meta data items represented as "subchunks" within the "wamd" chunk.
//
// Specifically, where all 16-bit and 32-bit values are little endian, 
// .WAV files begin with a RIFF/WAV header as follows:
//
//   char[4] = "RIFF"
//   Uint32  = length of file excluding 12-byte "RIFF/WAVE" header
//   char[4] = "WAVE"
//
// The format chunk used by Wildlife Acoustics is a standard PCM chunk
// as follows:
//
//    char[4] = "fmt "
//    Uint32  = length of format chunk following this field, = 16
//    Uint16  = "tag" = 0x0001 indicates uncompressed PCM encoding
//    Uint16  = channel count (1 for mono, 2 for stereo)
//    Uint32  = sample rate (samples per second)
//    Uint32  = byte rate (bytes per second = 2*channels*samplerate)
//    Uint16  = bytes per sample (2 for mono, 4 for stereo)
//    Uint16  = bits per sample = 16
//
// The data chunk is a standard .WAV data chunk as follows:
//
//    char[4] = "data"
//    Uint32  = length of chunk in bytes following this field
//    followed by 16-bit interleaved samples for each channel
//
// Note that Wildlife Acoustics may also include a "junk" chunk to padd the
// "data" chunk to a particular boundary.  Junk chunks if present should be
// ignored, as should any other unrecognized chunks.
//
// If meta data is present, it will be represented with a "wamd" chunk.
//
//   char[4] = "wamd"
//   Uint32  = length of chunk in bytes following this field
//
// The contents of the "wamd" chunk is a series of "subchunks".  Each
// "subchunk" is formatted as follows:
//
//   Uint16 = subchunk_id indicates the particular kind of information
//   Uint32 = length of subchunk in bytes following this field
//   followed by the specified number of bytes representing the value.
//
// The first subchunk must be a METATAG_VERSION subchunk.  The length is
// 2 bytes representing a 16-bit field containing the version META_VERSION.
// This description is only valid for version 1.  If the version field is
// different, the decoder should not attmept to parse the rest of the "wamd"
// chunk.
//
// After the METATAG_VERSION subchunk, other subchunks may follow in any order.
// It is likely that Wildlife Acoustics will define new subchunks in the future.
// If you are interested in extending meta information with your own subchunks,
// please let Wildlife Acoustics know so we can assign unique METATAGs if
// it makes sense for us to do so.
//
// The subchunks are defined as below.  All subchunks except the version field
// are optional and may not be present.  Many subchunks are represented as 
// ASCII or UNICODE strings which may or may not include nul termination. 
//
// One final note is that Wildlife Acoustics may in the future append a 
// "wamd" chunk to the end of WAC files to add additional information to 
// WAC-formatted files.
//
// Wildlife Acoustics reserves the right to make changes to this format at
// any time, but we generally try to preserve backward compatibility.
//

// Version value
#define META_VERSION 0x0001

// Meta tag values (subchunk_ids)
#define METATAG_VERSION         0x0000 // Version (16-bit le) = META_VERSION
#define METATAG_DEV_MODEL       0x0001 // Device Model, ASCII, e.g.: SM3
#define METATAG_DEV_SERIAL_NUM  0x0002 // Device Serial Number, ASCII
#define METATAG_SW_VERSION      0x0003 // Firmware version, ASCII e.g. R1.0.0
#define METATAG_DEV_NAME        0x0004 // Device's unique name, ASCII; (prefix)
#define METATAG_FILE_START_TIME 0x0005 // YYYY-MM-DD hh:mm:ss[.xxx][(+|-)hh:mm]
                                       // where .xxx is optional milliseconds
				       // where +hh:mm is optional UTC timezone
#define METATAG_GPS_FIRST       0x0006 // GPS at beginning of recording, ASCII
                                       // WGS84,nn.nnnnn,N,mmm.mmmmm,W[,alt]
#define METATAG_GPS_TRACK       0x0007 // GPS track log (not yet implemented)
#define METATAG_SOFTWARE        0x0008 // Desktop analysis software, ASCII
                                       // e.g. Kaleidoscope Pro 1.0.0
#define METATAG_LICENSE_ID      0x0009 // Desktop software licenses, ASCII
#define METATAG_USER_NOTES      0x000A // User-defined notes, ASCII 
#define METATAG_AUTO_ID         0x000B // Auto ID, ASCII; if present
#define METATAG_MANUAL_ID       0x000C // Manual ID, ASCII; if provided by user 
#define METATAG_VOICE_NOTE      0x000D // Voice Note, an encapsulated WAV file
#define METATAG_AUTO_ID_STATS   0x000E // Auto ID Statistics, ASCII
#define METATAG_TIME_EXPANSION  0x000F // Time expansion factor (16-bit le)
#define METATAG_DEV_PARAMS      0x0010 // device parameter block (for SM3s,
                                       // this is an embedded .PGM file)
#define METATAG_DEV_RUNSTATE    0x0011 // device runstate (e.g. for diagnostics)
#define METATAG_MIC_TYPE        0x0012 // microphone type, ASCII ch[,...]
#define METATAG_MIC_SENSITIVITY 0x0013 // microphone sensitivity (dBfs[,...])
#define METATAG_POS_LAST        0x0014 // position as last set by GPS or by user
                                       // same format as METATAG_GPS_FIRST
#define METATAG_TEMP_INT        0x0015 // internal temperature, ASCII w/ units
                                       // e.g. 12.3C
#define METATAG_TEMP_EXT        0x0016 // external temperature, ASCII w/ units
                                       // e.g. 12.3C
#define METATAG_HUMIDITY        0x0017 // humidity ASCII with units e.g. 80.2%RH
#define METATAG_LIGHT		0x0018 // light ASCII with units e.g. 200lm

#define METATAG_PADDING         0xFFFF // Optional padding for alignment, ignore

#endif // _WA_META_H
