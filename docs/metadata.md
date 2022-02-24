# Metadata Files

This document outlines all of the files from which we hope to extract metadata.

## FLAC Files

| Name                     | Supported | Location(s)  | Notes               |
| ------------------------ | --------- | ------------ | ------------------- |
| Sample Rate              | ✔️        | File header  |                     |
| Duration                 | ✔️        | File header  |                     |
| Total Samples            | ✔️        | File header  |                     |
| Channel Count            | ✔️        | File header  |                     |
| Bit Depth                | ✔️        | File header  |                     |
| Bits per Sample          | ✔️        | File header  |                     |

## WAV Files

| Name                     | Supported | Location(s)  | Notes               |
| ------------------------ | --------- | ------------ | ------------------- |
| Sample Rate              | :x:       | File header  |                     |
| Duration                 | :x:       | File header  |                     |
| Total Samples            | :x:       | File header  |                     |
| Channel Count            | :x:       | File header  |                     |
| Bit Depth                | :x:       | File header  |                     |
| Bits per Sample          | :x:       | File header  |                     |
| File Size                | :x:       | File header  |                     |

## Frontier Labs

### BAR-LT

| Name                     | Supported | Location(s)  | Notes               |
| ------------------------ | --------- | ------------ | ------------------- |
| Date Time                | :x:       | File name    |                     |
| UTC Offset               | :x:       | File name    |                     |
| Serial Number            | :x:       | File name    |                     |
| Microphone Serial Number | :x:       | File name    |                     |
| Longitude                | :x:       | File name    | Also in GPS_log.csv |
| Latitude                 | :x:       | File name    | Also in GPS_log.csv |
| Gain                     | :x:       | Support File | Log file            |
| Battery Voltage          | :x:       | Support File | Log file            |
| SD Card Serial           | :x:       | Support File | Log file            |
| Card Slot Number         | :x:       | Support File | Log file            |
| Battery Percentage       | :x:       | Support File | Log file            |
| SD Capacity (GB)         | :x:       | Support File | Log file            |
| SD Free Space (GB)       | :x:       | Support File | Log file            |
| ARU Firmware             | :x:       | Support File | Log file            |
| Device Type              | :x:       | Support File | Log file            |
| Power Type               | :x:       | Support File | Log file            |
| SD Card Manufacture Date | :x:       | Support File | Log file            |
| ARU Manufacture Date     | :x:       | Support File | Log file            |
| SD Card Speed            | :x:       | Support File | Log file            |
| SD Card Product Name     | :x:       | Support File | Log file            |
| SD Format Type           | :x:       | Support File | Log file            |
| SD Card Manufacture ID   | :x:       | Support File | Log file            |
| SD Card OemID            | :x:       | Support File | Log file            |
| SD Card Product Revision | :x:       | Support File | Log file            |
| SD Write Current Vmin    | :x:       | Support File | Log file            |
| SD Write Current Vmax    | :x:       | Support File | Log file            |
| SD Write B1 Size         | :x:       | Support File | Log file            |
| SD Write B2 Size         | :x:       | Support File | Log file            |

### BAR

## Open Acoustic Devices

### AudioMoth

| Name            | Supported | Location(s)  | Notes       |
| --------------- | --------- | ------------ | ----------- |
| Date Time       | :x:       | File name    |             |
| UTC Offset      | :x:       | Support File | Config file |
| Serial Number   | :x:       | Support File | Config file |
| Gain            | :x:       | File Header  |             |
| Battery Voltage | :x:       | File Header  |             |
| ARU Firmware    | :x:       | Support File | Config File |

## Wildlife Acoustics

### Song Meter SM4BAT

| Name            | Supported | Location(s)  | Notes        |
| --------------- | --------- | ------------ | ------------ |
| Date Time       | :x:       | File name    |              |
| Serial Number   | :x:       | File Header  |              |
| Longitude       | :x:       | File Header  |              |
| Latitude        | :x:       | File Header  |              |
| Battery Voltage | :x:       | Support File | Summary file |

### Song Meter SM4

| Name            | Supported | Location(s)  | Notes        |
| --------------- | --------- | ------------ | ------------ |
| Date Time       | :x:       | File Header  |              |
| Serial Number   | :x:       | File Header  |              |
| Longitude       | :x:       | File Header  |              |
| Latitude        | :x:       | File Header  |              |
| Battery Voltage | :x:       | Support File | Summary file |


### Song Meter Mini

| Name          | Supported | Location(s) | Notes |
| ------------- | --------- | ----------- | ----- |
| Serial Number | :x:       | File Header |       |

### Song Meter SM3

### Song Meter SM2

## Cornell Lab

### Swift

### SwiftOne
