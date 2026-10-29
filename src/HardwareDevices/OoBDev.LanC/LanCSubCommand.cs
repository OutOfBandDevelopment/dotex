namespace OoBDev.LanC;

/// <summary>The first byte of a LANC frame: which command table the second byte belongs to. Values follow <see href="http://www.boehmel.de/lanc.htm#byte0"/>.</summary>
public enum LanCSubCommand : byte
{
    //http://www.boehmel.de/lanc.htm#byte0
    CameraVtrNormal = 0b0001_1000, //0x18
    CameraSpecial = 0b0010_1000, //0x28
    VtrSpecial = 0b0011_1000, //0x38
    StillNormal = 0b0001_1110, //0x1E
}
