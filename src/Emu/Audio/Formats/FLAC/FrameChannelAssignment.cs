// <copyright file="FrameChannelAssignment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    /// <summary>
    /// https://xiph.org/flac/format.html#frame_header.
    /// </summary>
    public enum FrameChannelAssignment : byte
    {
        Mono = 1,
        LeftRight = 2,
        LeftRightCenter = 3,
        FrontLeftFrontRightBackLeftBackRight = 4,
        FrontLeftFrontRightFrontCenterBackLeftBackRight = 5,
        FrontLeftFrontRightFrontCenterLfeBackLeftBackRight = 6,
        FrontLeftFrontRightFrontCenterLfeBackCenterSideLeftSideRight = 7,
        FrontLeftFrontRightFrontCenterLfeBackLeftBackRightSideLeftSideRight = 8,

        // the aforementioned integer values are direct mappings
        // the following are not
        LeftPlusSideStereo = 9,
        RightPlusSideStereo = 10,
        MidPlusSideStereo = 11,
    }
}
