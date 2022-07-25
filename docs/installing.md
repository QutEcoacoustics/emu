# Getting started

EMU is a stand alone program*. You just need a copy of it and you can run it directly.

The latest releases can be found on the releases page: https://github.com/QutEcoacoustics/emu/releases

Download the release that is relevant for your platform. Extract in a place that is easy to access and then you can 
run the program from that spot.

For example:

 - a Windows user (`Anthony` for example) would download the `win-x64.zip` file.
 - They would unblock the zip file
 - They could extract EMU in the downloads folder
 - In that case emu could be run with:

```powershell
C:\Users\Anthony\Downloads\win-x64\emu.exe
```

## Platform specific tips

### Windows

- You should unblock the zip file after you download it, but _before_ you extract it

### Linux

- After extraction check the `emu` file has the execute permission.
  If needed apply the execute permission with `chmod u+x emu` inside the emu folder.

### MacOS

- MacOS's notarization process requires programs to be checked by Apple before they're allowed
  to run on you computer. We don't have the money to pay for this so there are two options:
  1. Bypass or disable the Gatekeeper: https://www.howtogeek.com/205393/gatekeeper-101-why-your-mac-only-allows-apple-approved-software-by-default/
  2. Clone this repository and build EMU yourself
    - Programs you make on your own computer are exempt from the security policy
    - See [CONTRIBUTING](../CONTRIBUTING.md) for instructions

## Latest versions

The latest changed may not yet be released. To get the very-latest version, clone the repo and build EMU yourself.
See [CONTRIBUTING](../CONTRIBUTING.md) for instructions.