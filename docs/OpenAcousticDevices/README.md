# Open Acoustic Devices

## Notes
- AudioMoth manual: https://github.com/OpenAcousticDevices/Application-Notes/blob/master/AudioMoth_Operation_Manual.pdf
- AudioMoth firmware repo
  -  https://github.com/OpenAcousticDevices/AudioMoth-Project/
  -  https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic
  -  It looks like you're meant to customize the firmware for your device
-  AudioMoth metadata 
   -  Uses basic WAVE metadata (`LIST`, `IART`, `ICMT`)
   -  Most of the metadata is in the `ICMT` chunk
      -  https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/36473b1e960230dd1c9af25e42271aeb72b88577/src/main.c#L420-L588
   -  The firmware that produced a file does not seem to be included in the metadata
   -  Metamoth has a complete implementation we heavily rely on: https://github.com/mbsantiago/metamoth/blob/main/src/metamoth/parsing.py
-  Config files:
   -  https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blob/36473b1e960230dd1c9af25e42271aeb72b88577/src/main.c#L592-L818