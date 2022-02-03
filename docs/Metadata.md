# Metadata Files

This document outlines all of the files from which we hope to extract metadata.
"PREFIX" indicates a user genertated prefix to the file.
"X" indicates which SD card a file was saved to.

## Frontier Labs

### BAR-LT

* GPS_log.csv

* logfile.txt

## Open Acoustic Devices

### AudioMoth

* CONFIG.txt

## Wildlife Acoustics

### Song Meter SM4BAT

* "PREFIX"_"X"_Summary.txt
  * Date
  * Time 
  * Latitude
  * Longitude
  * Power(V)
  * Temperature(C)
  * Number of files written since the previous line
  * Number of scrubbed .wav files since the previous line
  * Microphone type

* "PREFIX"_YYYYMMDD_hhmmss.sm4dump
  * Diagnostic data if something fails

### Song Meter SM4

* Summary.txt (Uses .csv format)
  * Date
  * Time 
  * Latitude
  * Longitude
  * Power(V)
  * Temperature(C)
  * Number of files written since the previous line
  * Microphone type attached to channel 0
  * Microphone type attached to channel 1

* Schedule file (in .SM4S format)

* Firmware file (in .SM4 format)

* Config file (in .SM4S format)

* "PREFIX"_YYYYMMDD_hhmmss.sm4dump


### Song Meter Mini

* .wav file metadata
  * Firmware version
  * Length
  * Loc position
  * Make
  * Model
  * Original filename
  * Sample rate
  * Serial
  * Temperature int
  * Timestamp
  * Audio settings
  * Prefix

* .zc file metadata
  * Firmware version
  * Length
  * Loc position
  * Make
  * Model
  * Original filename
  * Sample rate
  * Serial
  * Temperature int
  * Timestamp
  * Audio settings
  * Prefix

* Summary.txt

* Firmware file (in .smm format)

* Config file (in .config format)

* Noise files 

* Dump file
