# Metadata

The metadata commands in EMU are focused on manipulating and converting the various
metadata formats that exist.

The `metadata` command shows metadata gathered from audio files
**and** support files, in one consistent format.

The `metadata` command also has these subcommands:

- `show`: an alias for `metadata`
- `dump`: shows metadata from audio files only (no support files) in a low-level format
  - `dump` can show you more data but with less consistency - the names and values shown
    are (mostly) chosen by the vendor.
  - Dump currently supports;
    - Frontier Labs FLAC comments
    - Wildlife Acoustics WAMD blocks (and the embedded schedules!)
<!-- - `split`: splits metadata blocks into sidecar files
- `embed`: embeds metadata blocks into audio files
- `strip`: removes metadata blocks from audio files
- `edit`: edits metadata blocks in audio files -->

The metadata commands (mostly) support all the same formats as out other commands.
See [output](./output.md) for more information.

## Examples

### Show the metadata in a pretty format for the EMU test dataset

```sh
> emu metadata ./test/Fixtures
```

Sample results:

| WA SongMeter                                    | FL BAR                                             |
| ----------------------------------------------- | -------------------------------------------------- |
| ![SM4BAT metadata](media/metadata_show_sm4.png) | ![BAR LT metadata](media/metadata_show_bar_lt.png) |

### Extract all metadata from all wave files in an external HHD. Save the result into a CSV file

```sh
> emu metadata F:\Data\**\*.wav -F CSV -O metadata.csv
...
```

### Show low-level metadata in pretty format for all audio in the EMU test dataset

```sh
> emu metadata dump ./test/Fixtures
...
```

Sample results:

| WA SongMeter 3                              | WA SongMeter 4                               | FL BAR                                            |
| ------------------------------------------- | -------------------------------------------- | ------------------------------------------------- |
| [SM3 metadata](media/metadata_dump_sm3.png) | ![SM4 metadata](media/metadata_dump_sm4.png) | ![BAR LT metadata](media/metadata_dump_barlt.png) |

## Definitions

- All values are normalized to SI units where possible
  - any duration not explicitly formatted in the sexagesimal format (`HH:mm:ss`) will be in `seconds`
  - data sizes will always be in `bytes`
  - frequency will always be in `hertz`
  - energy will always be in `joules`
- Gain is the amount of amplification applied to the signal. It is measured in dB
  - The sensor can have a global gain setting
  - Each microphone/channel can have a gain setting
- Sensitivity is a calibration value applied to a microphone.
  It allows sample intensity to be converted back to a real physical quantity.
  Sensitivity settings do not affect how a recording is made.
  Sensitivity should be measured in relation to the microphone and sensor.
  Values reported are in dB.

## Supported metadata

Column definitions:

- _Name_: the name of the metadatum
- _Supported_: whether or not EMU can extract that metadatum from source material
- _Location (s): where a metadatum can be found. Some metadata are stored in multiple locations
  - The _Header_ is any data stored within a file itself. Such metadata do not strictly have to be at the start of the file. 
    e.g. the `wamd` chunk is often located at the end of an audio file
  - The _File_ is any data stored in the file system
  - The _Name_ is any metadata stored in the file's name
  - _Support_ files are additional files produced by the sensor that sit along side the audio recordings
- _Notes_: any extra information about the metadatum
- _Field_: which names we give to the metadatum after extraction
- _Units_: which unit we report the metadatum in after extraction

### FLAC Files

| Name              | Supported | Location(s) | Notes                 | Field | Units   |
| ----------------- | --------- | ----------- | --------------------- | ----- | ------- |
| Sample Rate       | ✔️         | Header      |                       |       | Hertz   |
| Duration          | ✔️         | Header      |                       |       | Seconds |
| Total Samples     | ✔️         | Header      |                       |       |         |
| Channel Count     | ✔️         | Header      |                       |       |         |
| Bit Depth         | ✔️         | Header      |                       |       |         |
| Bits per Second   | ✔️         | Header      |                       |       |         |
| File Size         | ✔️         | File        |                       |       |         |
| Computed Checksum | ✔️         | File        |                       |       |         |
| Embedded Checksum | ✔️         | Header      | MD5 of unencoded data |       |         |

### WAVE Files

| Name              | Supported | Location(s) | Notes | Field | Units   |
| ----------------- | --------- | ----------- | ----- | ----- | ------- |
| Sample Rate       | ✔️         | Header      |       |       | Hertz   |
| Duration          | ✔️         | Header      |       |       | Seconds |
| Total Samples     | ✔️         | Header      |       |       |         |
| Audio Format      | ✔️         | Header      |       |       |         |
| Channel Count     | ✔️         | Header      |       |       |         |
| Byte Rate         | ✔️         | Header      |       |       |         |
| Block Align       | ✔️         | Header      |       |       |         |
| Bit Depth         | ✔️         | Header      |       |       |         |
| Bits per Second   | ✔️         | Header      |       |       |         |
| File Size         | ✔️         | Header      |       |       |         |
| Computed Checksum | ✔️         | File        |       |       |         |


### Frontier Labs

Notes:

 - The datestamps in the file header are more accurate than the filename
   - FL determined that people preferred nice round dates that adhered to sensor schedules rather than properly accurate dates
 - FL do not seem to encode metadata in their WAVE files, only in their FLAC files.
   - Other manufacturers do encode metadata in their WAVE files
     - WA via their `wamd` chunk
     - OA do it as well

#### BAR-LT

| Name                     | Supported       | Location(s)           | Notes              | Field         | Units        |
| ------------------------ | --------------- | --------------------- | ------------------ | ------------- | ------------ |
| Date Time                | ✔️               | Name, Header, Support | Reclog             | StartDate     |              |
| RecordingStart           | ✔️               | Header                | First buffer write | TrueStartDate |              |
| RecordingEnd             | ✔️               | Header                | Last buffer write  | TrueEndDate   |              |
| UTC Offset               | ✔️               | Name, Header, Support |                    |               |              |
| Serial Number            | ✔️               | Header, Support       | Log file           |               |              |
| Microphone Type          | ✔️               | Header, Support       | Log file           |               |              |
| Microphone ID            | ✔️(Header, Name) | Header, Name, Support | Log file, Reclog   |               |              |
| Microphone Build Date    | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| Microphone Channel       | ✔️               | Header, Support       | Log file           |               |              |
| Longitude                | ✔️(Header, Name) | Header, Name, Support | GPS_log.csv        |               |              |
| Latitude                 | ✔️(Header, Name) | Header, Name, Support | GPS_log.csv        |               |              |
| Gain                     | ✔️(Header)       | Header, Name, Support | Log file, Reclog   |               | dB           |
| Battery Voltage          | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| Card Slot Number         | ❌               | Header, Support       | Log file, Reclog   |               |              |
| Battery Percentage       | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| Device Type              | ❌               | Header, Support       | Log file           |               |              |
| Power Type               | ✔️               | Support               | Log file           |               |              |
| Last Time Sync           | ✔️               | Header                |                    |               |              |
| ARU Firmware             | ✔️               | Header, Support       | Log file           |               |              |
| ARU Manufacture Date     | ❌               | Header, Support       | Log file           |               |              |
| SD Capacity (GB)         | ✔️               | Support               | Log file, Reclog   |               | bytes        |
| SD Free Space (GB)       | ❌               | Support               | Log file, Reclog   |               |              |
| SD Card Serial           | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Card Manufacture Date | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Card Speed            | ✔️               | Support               | Log file           |               | bytes/second |
| SD Card Product Name     | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Format Type           | ✔️               | Support               | Log file           |               |              |
| SD Card Manufacture ID   | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Card OemID            | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Card Product Revision | ✔️               | Header, Support       | Log file, Reclog   |               |              |
| SD Write Current Vmin    | ✔️               | Support               | Log file           |               |              |
| SD Write Current Vmax    | ✔️               | Support               | Log file           |               |              |
| SD Write Bl Size         | ✔️               | Support               | Log file           |               |              |
| SD Erase Bl Size         | ✔️               | Support               | Log file           |               |              |


#### BAR

### Open Acoustic Devices

#### AudioMoth

| Name            | Supported | Location(s) | Notes       | Field | Units |
| --------------- | --------- | ----------- | ----------- | ----- | ----- |
| Date Time       | ✔️         | Name        |             |       |       |
| UTC Offset      | ✔️         | Support     | Config file |       |       |
| Serial Number   | ❌         | Support     | Config file |       |       |
| Gain            | ❌         | Header      |             |       |       |
| Battery Voltage | ❌         | Header      |             |       |       |
| ARU Firmware    | ❌         | Support     | Config File |       |       |

### Wildlife Acoustics

#### Song Meter SM4/SM4BAT/SM3

| Name                     | Supported | Location(s)  | Notes                                   | Field                  | Units             |
| ------------------------ | --------- | ------------ | --------------------------------------- | ---------------------- | ----------------- |
| Date Time                | ✔️         | Name, Header |                                         | LocalStartDate         |                   |
| High Precision Date Time | ✔️         | Name, Header |                                         | StartDate              |                   |
| Sensor Make              | ✔️         | Header       |                                         |                        |                   |
| Sensor Model             | ✔️         | Header       |                                         |                        |                   |
| Sensor Name              | ✔️         | Header       |                                         |                        |                   |
| Firmware                 | ✔️         | Header       |                                         |                        |                   |
| Serial Number            | ✔️         | Header       |                                         |                        |                   |
| Longitude                | ✔️         | Header       |                                         |                        |                   |
| Latitude                 | ✔️         | Header       |                                         |                        |                   |
| Temperature (internal)   | ✔️         | Header       |                                         |                        | °C                |
| Temperature (external)   | ❔         | Header       | No test files currently available       |                        | °C                |
| Light                    | ❔         | Header       | No test files currently available       |                        | Candela           |
| Humidity                 | ❔         | Header       | No test files currently available       |                        | relative humidity |
| Battery Voltage          | ❌         | Support      | Summary file, need examples!            |                        |                   |
| WAMD metadata block      | ✔️ish      | Header       | Most fields supported                   |                        |                   |
| Embedded schedule        | ✔️         | Header       | The schedule used to program the sensor |                        |                   |
| Microphone Gain          | ✔️         | Header       | See notes                               | Microphone.Gain        | dB                |
| Microphone Sensitivity   | ✔️         | Header       | See notes                               | Microphone.Sensitivity | dB                |

Notes: 

- Microphone gain is determined from the embedded schedule file that was used to create a recording
  - ON SM4s the value is `(Preamp On? ? 26 dB : 0 ) + Gain` per channel
    -  Unless an external microphone is used as the preamp only applies to the internal microphone
  - ON SM3s the value is 
    - the first and only GAIN entry in the schedule (otherwise fail)
    - the value of GAIN entry if specified in dB, or
    - if the GAIN value is set to automatic then the value is 24 dB IFF the microphone is not a hydrophone,
      as per page 36 of the [SM3 manual](https://www.wildlifeacoustics.com/uploads/user-guides/SM3-USER-GUIDE-20200805.pdf).

#### Song Meter Mini

TODO: Need more example files!

| Name          | Supported | Location(s) | Notes | Field | Units |
| ------------- | --------- | ----------- | ----- | ----- | ----- |
| Serial Number | ❌         | Header      |       |       |       |


#### Song Meter SM2

TODO: Need more example files!

### Cornell Lab

TODO: Need more example files!

#### Swift

TODO: Need more example files!

#### SwiftOne

TODO: Need more example files!