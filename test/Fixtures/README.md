# Fixtures

## Folders

Place fixtures in sub-directories that match the following pattern:

```shell
<VENDOR>_<MODEL>/<FIRMWARE_VERSION>_<NAME>/
```

For example:

```shell
./FL_BAR_LT/3.14_PreallocatedHeader"
```

- The vendor is `FL` (Frontier Labs)
- The model is `BAR_LT`
- The firmware version is `3.14`
- The name is `PreallocatedHeader` which is a short description of the reason this file was included as a fixture

## Metadata

### Fixtures data

When you add a fixture be sure to also add in the metadata for that fixture in the fixtures metadata file
(currently `Fixtures.csv`)

### Fixtures provenance

Each folder that stored a fixture **MUST** contain a `README.md` file with provenance information about the example.

An example provenance `README.md` follows:

```markdown
# Provenance

This file was sourced from the Australian Acoustic Observatory under a 
Creative Commons By Attribution v4.0 license.

Site: 64
Point: 253
Memory Card: 337

## Fault information:

Well Known Problem FL010.
```

A provenance README must contain at least:

- The owner of the data
- The license under which the data was released to EMU
- A description of while the file was included in our Fixtures set
- Any artificial manipulations made to the files (typically file size reductions)

Additional optional information includes:

- A URL to the data source
- Any other distinguishing metadata about the data
- A DOI or citation if required

## Compressed fixtures

Fixtures can be committed as `.zip` archives to reduce repository size. Test setup recursively scans `test/Fixtures`
for `.zip` files and prepares fixtures during the build step before tests run.

For example, if `FixturePath` is:

```text
FL_BAR_LT/_GuanoSample/33901_BigHarbourIslandA1_20250710T130000-0300.wav
```

then this archive is supported:

```text
FL_BAR_LT/_GuanoSample/33901_BigHarbourIslandA1_20250710T130000-0300.wav.zip
```

Preparation is incremental: only missing fixtures, or fixtures older than their `.zip` archive, are extracted.
Extraction is also parallelized to speed up preparation on larger fixture sets.

If you use a compressed fixture, you **MUST** .gitignore the uncompressed fixture path.
