
# FL010

The fix for the FL010 problem was based off of this program from Frontier Labs:

```cpp
#include <iostream>
#include <iomanip>
#include <stdio.h>
#include <string.h>

using namespace std;


static float readFirmware(FILE * f)
{
    char fware_string[15];
    char fware_ver[5];

    float ver;

    int numBytesRead = 0;

    fseek(f,213,SEEK_SET);
    numBytesRead = fread(fware_string, 1, 15, f);
    if( numBytesRead < 15 ){ cout << "[ Error reading file! ]"; return -1; }

    if( strncmp(fware_string, "FirmwareVersion", 15 ) != 0 ){
        cout << "\nError: FirmwareVersion tag incorrect {" << fware_string << "} ";
        return -1;
    }

    fseek(f,230,SEEK_SET);
    numBytesRead = fread(fware_ver, 1, 4, f);
    if( numBytesRead < 4 ){ cout << "[ Error reading file! ]"; return -1; }

    fware_ver[4] = '\0';                                  // terminate the string
    ver = std::stof(fware_ver);
    return ver;
}

static unsigned int readLength(FILE * f)
{

    unsigned char length_str[5];
    unsigned int length = 0;
    int numBytesRead = 0;

    fseek(f,22,SEEK_SET);
    numBytesRead = fread(length_str, 1, 4, f);
    if( numBytesRead < 4 ){ cout << "[ Error reading file! ]"; return 0; }

    length = (unsigned int)length_str[0] << 24;
    length |= (unsigned int)length_str[1] << 16;
    length |= (unsigned int)length_str[2] << 8;
    length |= (unsigned int)length_str[3] << 0;

    return length;
}



static int writeLength(FILE * f, unsigned int length)
{
    unsigned char length_str[5];
    int numBytesWritten = 0;

    length_str[0] = (unsigned char)((length >> 24) & 0xFF);
    length_str[1] = (unsigned char)((length >> 16) & 0xFF);
    length_str[2] = (unsigned char)((length >> 8) & 0xFF);
    length_str[3] = (unsigned char)((length) & 0xFF);

    fseek(f,22,SEEK_SET);
    numBytesWritten = fwrite(length_str, 1, 4, f);
    if( numBytesWritten != 4 ){ cout << "[ Error writing to file! ]"; return -1; }

    return numBytesWritten;
}




int main(int argc, char *argv[])
{

    FILE * f;

    cout << "FLAC FIXER ";

    if( argc < 2 ){ cout << "[ Error: please specify a filename. ]"; return -1; }

    cout << argv[1] << " ";

    f = fopen(argv[1],"r+");
    if( f == NULL ){ cout << "[ Error: file not found. ]"; return -1; }

    /////////////////////////////////////////////////////////////////////////////////////
    ///            Read the file length
    /////////////////////////////////////////////////////////////////////////////////////

    unsigned int length = readLength( f );
    if( length == 0 ){ return -1; }
    cout << "Length: " << length << " ";

    /////////////////////////////////////////////////////////////////////////////////////
    ///            Read the firmware version
    /////////////////////////////////////////////////////////////////////////////////////

    float ver = readFirmware(f);
    if( ver < 0) { return -1; }

    cout << "FirmwareVersion: " << fixed << setprecision(2) << ver << " ... ";
    if ( ver < 3.17 || ver >= 3.28 )
    {
        cout << "[ No fix needed ]";
        return 0;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    ///            Write back the corrected file length
    /////////////////////////////////////////////////////////////////////////////////////
    length = length / 2;
    if( writeLength( f , length ) < 0 ){ return -1; }


    /////////////////////////////////////////////////////////////////////////////////////
    ///            Read back the file length to check
    /////////////////////////////////////////////////////////////////////////////////////

    unsigned int fixed_length = readLength( f );
    if( fixed_length == 0 ){ return -1; }
    cout << "Fixed Length: " << fixed_length << " ";
    if( fixed_length == length)
    {
        cout << "[ FIXED ]";
    }
    else
    {
        cout << "[ UNKNOWN ERROR! ]";
    }

    fclose(f);

    return 0;
}

```
