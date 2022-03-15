# Fixtures


## Folders

Place fixtures in sub-directories that match the following pattern:

```
<VENDOR>_<MODEL>/<FIRMWARE_VERSION>_<NAME>/
```

For example:

```
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

# Fault information:

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
