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

| Name            | Supported | Location(s) | Notes |
| --------------- | --------- | ----------- | ----- |
| Sample Rate     | ❌         | Header      |       |
| Duration        | ❌         | Header      |       |
| Total Samples   | ❌         | Header      |       |
| Channel Count   | ❌         | Header      |       |
| Bit Depth       | ❌         | Header      |       |
| Bits per Sample | ❌         | Header      |       |
| File Size       | ❌         | File        |       |

## Frontier Labs

### BAR-LT

| Name                     | Supported | Location(s)     | Notes       |
| ------------------------ | --------- | --------------- | ----------- |
| Date Time                | ❌         | Name            |             |
| UTC Offset               | ❌         | Name            |             |
| Serial Number            | ❌         | Header, Support |             |
| Microphone Serial Number | ❌         | Header, Support |             |
| Longitude                | ❌         | Name, Support   | GPS_log.csv |
| Latitude                 | ❌         | Name, Support   | GPS_log.csv |
| Gain                     | ❌         | Header, Support | Log file    |
| Battery Voltage          | ❌         | Header, Support | Log file    |
| SD Card Serial           | ❌         | Header, Support | Log file    |
| Card Slot Number         | ❌         | Header, Support | Log file    |
| Battery Percentage       | ❌         | Header, Support | Log file    |
| SD Capacity (GB)         | ❌         | Header, Support | Log file    |
| SD Free Space (GB)       | ❌         | Header, Support | Log file    |
| ARU Firmware             | ❌         | Header, Support | Log file    |
| Device Type              | ❌         | Header, Support | Log file    |
| Power Type               | ❌         | Support         | Log file    |
| SD Card Manufacture Date | ❌         | Header, Support | Log file    |
| ARU Manufacture Date     | ❌         | Header, Support | Log file    |
| SD Card Speed            | ❌         | Header, Support | Log file    |
| SD Card Product Name     | ❌         | Header, Support | Log file    |
| SD Format Type           | ❌         | Header, Support | Log file    |
| SD Card Manufacture ID   | ❌         | Header, Support | Log file    |
| SD Card OemID            | ❌         | Header, Support | Log file    |
| SD Card Product Revision | ❌         | Header, Support | Log file    |
| SD Write Current Vmin    | ❌         | Header, Support | Log file    |
| SD Write Current Vmax    | ❌         | Header, Support | Log file    |
| SD Write B1 Size         | ❌         | Header, Support | Log file    |
| SD Write B2 Size         | ❌         | Header, Support | Log file    |

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
