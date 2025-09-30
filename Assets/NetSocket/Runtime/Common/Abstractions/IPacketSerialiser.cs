namespace UFrame.NetSocket
{
    public interface IPacketSerialiser
    {
        T Deserialise<T>(byte[] packetBytes,int offset,int length);
        byte[] Serialize<T>(T packet);
    }
}