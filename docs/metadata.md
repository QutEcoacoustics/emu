# Metadata Files

This document outlines all of the files from which we hope to extract metadata.


## FLAC Files

| Name            | Supported | Location(s) | Notes |
| --------------- | --------- | ----------- | ----- |
| Sample Rate     | ✔️         | Header      |       |
| Duration        | ✔️         | Header      |       |
| Total Samples   | ✔️         | Header      |       |
| Channel Count   | ✔️         | Header      |       |
| Bit Depth       | ✔️         | Header      |       |
| Bits per Sample | ✔️         | Header      |       |
| File Size       | ❌         | File        |       |

## WAVE Files

| Name                     | Supported | Location(s)  | Notes               |
| ------------------------ | --------- | ------------ | ------------------- |
| Sample Rate              | ✔️        | Header  |                     |
| Duration                 | ✔️        | Header  |                     |
| Total Samples            | ✔️        | Header  |                     |
| Audio Format             | ✔️        | Header  |                     |
| Channel Count            | ✔️        | Header  |                     |
| Byte Rate                | ✔️        | Header  |                     |
| Block Align              | ✔️        | Header  |                     |
| Bit Depth                | ✔️        | Header  |                     |
| Bits per Sample          | ✔️        | Header  |                     |
| File Size                | ✔️        | Header  |                     |


## Frontier Labs

### BAR-LT

| Name                     | Supported | Location(s)     | Notes       |
| ------------------------ | --------- | --------------- | ----------- |
| Date Time                | ✔️(Header) | Name, Header    |             |
| UTC Offset               | ✔️(Header) | Name, Header    |             |
| Serial Number            | ✔️         | Header, Support | Log file    |
| Microphone Serial Number | ✔️(Header) | Header, Support | Log file    |
| Microphone Type          | ✔️(Header) | Header, Support | Log file    |
| Microphone ID            | ✔️(Header) | Header, Support | Log file    |
| Microphone Build Date    | ✔️(Header) | Header, Support | Log file    |
| Microphone Channel       | ✔️(Header) | Header, Support | Log file    |
| Longitude                | ✔️(Header) | Header, Name, Support   | GPS_log.csv |
| Latitude                 | ✔️(Header) | Header, Name, Support   | GPS_log.csv |
| Gain                     | ✔️(Header) | Header, Support | Log file    |
| Battery Voltage          | ✔️(Header) | Header, Support | Log file    |
| Card Slot Number         | ❌         | Header, Support | Log file    |
| Battery Percentage       | ✔️(Header) | Header, Support | Log file    |
| Device Type              | ❌         | Header, Support | Log file    |
| Power Type               | ✔️         | Support         | Log file    |
| Last Time Sync           | ✔️         | Header          |             |
| ARU Firmware             | ✔️         | Header, Support | Log file    |
| ARU Manufacture Date     | ❌         | Header, Support | Log file    |
| SD Capacity (GB)         | ✔️(Support)| Support         | Log file    |
| SD Free Space (GB)       | ❌         | Support         | Log file    |
| SD Card Serial           | ✔️         | Header, Support | Log file    |
| SD Card Manufacture Date | ✔️         | Header, Support | Log file    |
| SD Card Speed            | ✔️         | Support         | Log file    |
| SD Card Product Name     | ✔️         | Header, Support | Log file    |
| SD Format Type           | ✔️         | Support         | Log file    |
| SD Card Manufacture ID   | ✔️         | Header, Support | Log file    |
| SD Card OemID            | ✔️         | Header, Support | Log file    |
| SD Card Product Revision | ✔️         | Header, Support | Log file    |
| SD Write Current Vmin    | ✔️         | Support         | Log file    |
| SD Write Current Vmax    | ✔️         | Support         | Log file    |
| SD Write Bl Size         | ✔️         | Support         | Log file    |
| SD Erase Bl Size         | ✔️         | Support         | Log file    |

### BAR

## Open Acoustic Devices

### AudioMoth

| Name            | Supported | Location(s) | Notes       |
| --------------- | --------- | ----------- | ----------- |
| Date Time       | ❌         | Name        |             |
| UTC Offset      | ❌         | Support     | Config file |
| Serial Number   | ❌         | Support     | Config file |
| Gain            | ❌         | Header      |             |
| Battery Voltage | ❌         | Header      |             |
| ARU Firmware    | ❌         | Support     | Config File |

## Wildlife Acoustics

### Song Meter SM4BAT

| Name            | Supported | Location(s) | Notes        |
| --------------- | --------- | ----------- | ------------ |
| Date Time       | ❌         | Name        |              |
| Serial Number   | ❌         | Header      |              |
| Longitude       | ❌         | Header      |              |
| Latitude        | ❌         | Header      |              |
| Battery Voltage | ❌         | Support     | Summary file |

### Song Meter SM4

| Name            | Supported | Location(s) | Notes        |
| --------------- | --------- | ----------- | ------------ |
| Date Time       | ❌         | Header      |              |
| Serial Number   | ❌         | Header      |              |
| Longitude       | ❌         | Header      |              |
| Latitude        | ❌         | Header      |              |
| Battery Voltage | ❌         | Support     | Summary file |

### Song Meter Mini

| Name          | Supported | Location(s) | Notes |
| ------------- | --------- | ----------- | ----- |
| Serial Number | ❌         | Header      |       |

### Song Meter SM3

### Song Meter SM2

## Cornell Lab

### Swift

### SwiftOne
