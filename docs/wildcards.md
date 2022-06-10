# Wildcards

Wildcards - like in the game Uno - are characters that represent a match for any other character.

Some common wildcards are listed below:

- slashes (forward for Linux `/` and backwards for Windows `\`) denote directories (folders)
- the asterisk (`*`) will match any number of characters in one name 
- two asterisks (`**`) will match any sub-directory (any nested folder)

With these wild cards, EMU commands like `rename` and `metadata` can work on multiple files at once
in a powerful way.

For example:

- `**/*.mp3` would find any MP3 in any sub-folder, in the current folder
- `MoggillCreek*.flac` would find any FLAC file with the `MoggillCreek` prefix in the current folder
- `F:\SturtDesert\**\2022*.wav` would find any WAVE file that has the prefix `2022` in any sub-folder on an external hard drive
- Commands like `rename` and `metadata` also support multiple patterns.
  Supplying `**/*.flac **/*.wav **/*.mp3` would match any MP3, FLAC, or WAVE file in any folder, in the current folder
  

