/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge.Library.Interfaces
{
    public interface IConvert2<SourceType>
    {
        SourceType DownstreamConvert(byte[] messagePayload);
        byte[] UpstreamConvert(SourceType dt);
    }

}
