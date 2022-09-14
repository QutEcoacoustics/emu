# Useful FLAC resources

- FLAC spec https://xiph.org/flac/format.html#frame_header
- `ffprobe -show_frames <file>` to print frames
-  `flac -a <file>` to save frames to a `.ana` file
-  a high quality flac decoder written in go https://github.com/eaburns/flac/tree/9a6fb92396d1ba6412b82819435dca0b46f959fb
-  the flac decoder source https://github.com/xiph/flac/blob/master/src/libFLAC/stream_decoder.c