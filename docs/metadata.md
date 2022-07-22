# Metadata Files

This document outlines all of the files from which we hope to extract metadata.


## Examples

### Extract all metadata from all wave files in an external HHD. Save the result into a CSV file

```sh
> emu metadata F:\Data\**\*.wav -F CSV -O metadata.csv
...
```


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

#### Song Meter SM4BAT

| Name            | Supported | Location(s)  | Notes        | Field     | Units |
| --------------- | --------- | ------------ | ------------ | --------- | ----- |
| Date Time       | ✔️         | Name, Header |              | StartDate |       |
| Start Date      | ✔️         | Name, Header |              | StartDate |       |
| Sensor Name     | ✔️         | Header       |              |           |       |
| Firmware        | ✔️         | Header       |              |           |       |
| Serial Number   | ✔️         | Header       |              |           |       |
| Longitude       | ✔️         | Header       |              |           |       |
| Latitude        | ✔️         | Header       |              |           |       |
| Temperature     | ✔️         | Header       |              |           | °C    |
| Battery Voltage | ❌         | Support      | Summary file |           |       |


#### Song Meter SM4

| Name            | Supported | Location(s) | Notes        | Field | Units |
| --------------- | --------- | ----------- | ------------ | ----- | ----- |
| Date Time       | ✔️         | Header      |              |       |       |
| Sensor Name     | ✔️         | Header      |              |       |       |
| Firmware        | ✔️         | Header      |              |       |       |
| Serial Number   | ✔️         | Header      |              |       |       |
| Longitude       | ✔️         | Header      |              |       |       |
| Latitude        | ✔️         | Header      |              |       |       |
| Temperature     | ✔️         | Header      |              |       | °C    |
| Battery Voltage | ❌         | Support     | Summary file |       |       |

#### Song Meter Mini

| Name          | Supported | Location(s) | Notes | Field | Units |
| ------------- | --------- | ----------- | ----- | ----- | ----- |
| Serial Number | ❌         | Header      |       |       |       |

#### Song Meter SM3

#### Song Meter SM2

### Cornell Lab

#### Swift

#### SwiftOne
