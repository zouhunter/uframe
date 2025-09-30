using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace UFrame.NetSocket
{
    public class ClientPacketProcessor : IClientPacketProcessor
    {
        private readonly PacketDataPool bytePool;
        private readonly ILogger logger;
        private readonly ClientBuilderOptions options;
        private readonly IPacketHandlers packetHandlers;
        private readonly IPacketSerialiser packetSerialiser;
        private readonly ObjectPool<ISender> tcpSenderObjectPool;
        private readonly ObjectPool<ISender> udpSenderObjectPool;

        private IUdpSocketSender _udpSocketSender;
        private ObjectPool<PacketContext> _packetContextObjectPool;
        private PacketSendPool packetSendPool;
        private event Action mainThreadEvent;
        public ClientPacketProcessor(ClientBuilderOptions options,
            IPacketSerialiser packetSerialiser,
            ILogger logger,
            IPacketHandlers packetHandlers)
        {
            this.options = options;
            this.packetSerialiser = packetSerialiser;
            this.logger = logger;
            this.packetHandlers = packetHandlers;
            packetSendPool = new PacketSendPool(options.PacketPartSize);
            tcpSenderObjectPool = new ObjectPool<ISender>(() => new TcpSender(packetSendPool, packetSerialiser));
            udpSenderObjectPool = new ObjectPool<ISender>(() => new UdpSender(packetSerialiser, _udpSocketSender, packetSendPool));
            _packetContextObjectPool = new ObjectPool<PacketContext>(() => new PacketContext()
            {
                Serializer = this.packetSerialiser,
                onRelease = this.OnReleasePacketContext
            });;
            bytePool = new PacketDataPool(options.PacketPartSize);
        }

        private void OnReleasePacketContext(PacketContext context)
        {
            _packetContextObjectPool.Push(context);
        }

        public void MainThreadRefresh()
        {
            if (mainThreadEvent != null)
            {
                mainThreadEvent.Invoke();
                mainThreadEvent = null;
            }
        }

        public void Process(Socket socket)
        {
            var packetData = bytePool.GetEmptyPacketData();
            var sender = tcpSenderObjectPool.Pop();

            try
            {
                int recevieNum = socket.Receive(packetData.buffer);
                packetData.offset = 0;
                packetData.length = recevieNum;
                var tcpSender = sender as TcpSender;
                tcpSender.Socket = socket;
                bytePool.EnqueueDataPacket(packetData);
                Process(sender);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                tcpSenderObjectPool.Push(sender);
            }
        }

        public void Process(UdpReceiveResult data)
        {
            var sender = udpSenderObjectPool.Pop();

            try
            {
                var buffer = data.Buffer;

                var udpSender = sender as UdpSender;
                udpSender.RemoteEndpoint = data.RemoteEndPoint;

                Process(buffer, sender);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                udpSenderObjectPool.Push(sender);
            }
        }

        public void SetUdpSocketSender(IUdpSocketSender socketSender)
        {
            _udpSocketSender = socketSender;
        }

        private void Process(byte[] buffer, ISender sender)
        {
            for (int i = 0; i < buffer.Length;)
            {
                var packetData = bytePool.GetEmptyPacketData();
                var maxSize = Math.Min(packetData.buffer.Length, buffer.Length);
                packetData.CopyBuff(buffer, i, maxSize);
                bytePool.EnqueueDataPacket(packetData);
                i += maxSize;
            }
            Process(sender);
        }

        private void Process(ISender sender)
        {
            while (bytePool.CheckExistData(out int dataLength))
            {
                var context = _packetContextObjectPool.Pop();
                context.Sender = sender;
                context.PacketLength = dataLength;
                if (context.PacketBytes == null || context.PacketBytes.Length < dataLength)
                    context.PacketBytes = new byte[dataLength + bytePool.BufferSize];
                if (bytePool.CollectData(context.PacketBytes, dataLength))
                {
                    context.Opcode = System.BitConverter.ToUInt16(context.PacketBytes, 4);
                    if (packetHandlers.GetPacketHandlers().TryGetValue(context.Opcode, out var packetHandler))
                    {
                        packetHandler.Handle(context,true);
                        //logger.LogError("packetHandler.MainThreadRefresh");
                        mainThreadEvent += packetHandler.MainThreadRefresh;
                    }
                }
                //_packetContextObjectPool.Push(context);
            }
            //    var bytesRead = 0;
            //    var currentPosition = 0;
            //    while (bytesRead < buffer.Length)
            //    {
            //        var packetTypeNameLength = packetSerialiser.CanReadOpcode
            //            ? BitConverter.ToInt32(buffer, currentPosition)
            //            : 0;

            //        if (packetSerialiser.CanReadOpcode) currentPosition += 4;

            //        var packetSize = packetSerialiser.CanReadLength
            //            ? BitConverter.ToInt32(buffer, currentPosition)
            //            : 0;

            //        if (packetSerialiser.CanReadLength) currentPosition += 4;

            //        var packetTypeName = "Default";

            //        if (packetSerialiser.CanReadOpcode)
            //        {
            //            if (buffer.Length - currentPosition < packetTypeNameLength)
            //            {
            //                return;
            //            }

            //            packetTypeName = Encoding.ASCII.GetString(buffer, currentPosition, packetTypeNameLength);
            //            currentPosition += packetTypeNameLength;

            //            if (string.IsNullOrEmpty(packetTypeName))
            //            {
            //                logger.Error(new Exception("Packet was lost - Invalid"));
            //                return;
            //            }
            //        }

            //        var packetHandler = packetHandlers.GetPacketHandlers()[packetTypeName];

            //        if (packetSerialiser.CanReadLength && buffer.Length - bytesRead < packetSize)
            //        {
            //            logger.Error(new Exception("Packet was lost"));
            //            return;
            //        }

            //        var context = _packetContextObjectPool.Pop();
            //        context.Sender = sender;

            //        if (packetSerialiser.CanReadOffset)
            //        {
            //            context.PacketBytes = buffer;
            //            //packetHandler.Handle(buffer, currentPosition, packetSize, context).GetAwaiter().GetResult();
            //            //Not required/supported right now
            //        }
            //        else
            //        {
            //            var packetBytes = new byte[packetSize];
            //            Buffer.BlockCopy(buffer, currentPosition, packetBytes, 0, packetSize);
            //            context.PacketBytes = packetBytes;
            //            packetHandler.Handle(context).GetAwaiter().GetResult();
            //        }

            //        _packetContextObjectPool.Push(context);

            //        currentPosition += packetSize;
            //        bytesRead += packetSize + packetTypeNameLength;

            //        if (packetSerialiser.CanReadOpcode) bytesRead += 4;
            //        if (packetSerialiser.CanReadLength) bytesRead += 4;
            //    }
        }
    }

}