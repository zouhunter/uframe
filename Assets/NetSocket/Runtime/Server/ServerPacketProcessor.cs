using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public class ServerPacketProcessor : IServerPacketProcessor
    {
        private readonly ILogger logger;
        private readonly List<IMiddlewareHandler> middlewares;
        private readonly ObjectPool<PacketContext> packetContextObjectPool;
        private readonly IPacketHandlers packetHandlers;
        private readonly IPacketSerialiser packetSerialiser;
        private readonly IServerInformation serverInformation;
        private readonly ObjectPool<ISender> tcpSenderObjectPool;
        private readonly ObjectPool<ISender> udpSenderObjectPool;
        private readonly PacketDataPool bytePool;
        private IUdpSocketSender _socketSender;
        private PacketSendPool m_packetSendPool;

        public ServerPacketProcessor(ServerBuilderOptions options,
            ILogger logger,
            IPacketHandlers packetHandlers,
            IServerInformation serverInformation,
            IPacketSerialiser packetSerialiser,
            IEnumerable<IMiddlewareHandler> middlewares)
        {
            this.logger = logger;
            this.packetHandlers = packetHandlers;
            this.serverInformation = serverInformation;
            this.packetSerialiser = packetSerialiser;
            this.middlewares = middlewares?.ToList();
            m_packetSendPool = new PacketSendPool(options.PacketPartSize);
            tcpSenderObjectPool = new ObjectPool<ISender>(()=> new TcpSender(m_packetSendPool, packetSerialiser));
            udpSenderObjectPool = new ObjectPool<ISender>(()=> new UdpSender(packetSerialiser, _socketSender, m_packetSendPool));
            packetContextObjectPool = new ObjectPool<PacketContext>(()=> {
                var packetContext = new PacketContext();
                packetContext.Serializer = packetSerialiser;
                packetContext.onRelease = OnReleasePacketContext;
                return packetContext;
            });
            bytePool = new PacketDataPool(options.PacketPartSize);
        }

        private void OnReleasePacketContext(PacketContext context)
        {
            packetContextObjectPool.Push(context);
        }

        public async Task ProcessFromBuffer(ISender sender,
            byte[] buffer,
            int offset = 0,
            int length = 0,
            bool isTcp = true)
        {
            for (int i = offset; i < offset + length;)
            {
                var packetData = bytePool.GetEmptyPacketData();
                var maxSize = Math.Min(packetData.buffer.Length, length);
                packetData.CopyBuff(buffer, i, maxSize);
                bytePool.EnqueueDataPacket(packetData);
                i += maxSize;
            }
            await Task.Run(() => Process(sender, isTcp));

            //var bytesRead = 0;
            //var currentPosition = offset;

            //if (length == 0)
            //	length = buffer.Length;

            //while (bytesRead < length)
            //{
            //	var packetNameSize = packetSerialiser.CanReadOpcode
            //		? BitConverter.ToInt32(buffer, currentPosition)
            //		: 0;

            //	if (packetSerialiser.CanReadOpcode) currentPosition += 4;

            //	var packetSize = packetSerialiser.CanReadLength
            //		? BitConverter.ToInt32(buffer, currentPosition)
            //		: 0;

            //	if (packetSerialiser.CanReadLength) currentPosition += 4;

            //	try
            //	{
            //		var packetTypeName = "Default";

            //		if (packetSerialiser.CanReadOpcode)
            //			packetTypeName = Encoding.ASCII.GetString(buffer, currentPosition, packetNameSize);

            //		var packetHandlerDictionary = packetHandlers.GetPacketHandlers();

            //		if (!packetHandlerDictionary.ContainsKey(packetTypeName))
            //		{
            //			logger.LogWarning(
            //				$"Could not handle packet {packetTypeName}. Make sure you have registered a handler");
            //			return;
            //		}

            //		var packetHandler = packetHandlerDictionary[packetTypeName];

            //		if (string.IsNullOrEmpty(packetTypeName))
            //		{
            //			if (isTcp)
            //				serverInformation.InvalidTcpPackets++;
            //			else
            //				serverInformation.InvalidUdpPackets++;

            //			logger.Error(new Exception("Packet was lost - Invalid"));
            //			return;
            //		}

            //		if (packetSerialiser.CanReadOpcode) currentPosition += packetNameSize;

            //		var packetContext = packetContextObjectPool.Pop();
            //		packetContext.Opcode = packetTypeName;
            //		packetContext.Sender = sender;
            //		packetContext.Serialiser = packetSerialiser;
            //		packetContext.Handler = packetHandler;

            //		if (packetSerialiser.CanReadOffset)
            //		{
            //			//todo: Fix
            //			//packetContext.PacketBytes = buffer;
            //			//packetHandler.Handle(buffer, currentPosition, packetSize, packetContext).GetAwaiter().GetResult();
            //		}
            //		else
            //		{
            //			packetContext.PacketBytes = new byte[packetSize];
            //			Buffer.BlockCopy(buffer, currentPosition, packetContext.PacketBytes, 0, packetSize);
            //		}

            //		logger.LogDebug("packetHandler.Handle(packetContext)");

            //                 await packetHandler.Handle(packetContext);

            //		packetContextObjectPool.Push(packetContext);
            //	}
            //	catch (Exception e)
            //	{
            //		logger.Error(e);
            //	}

            //	if (packetSerialiser.CanReadLength) currentPosition += packetSize;

            //	bytesRead += packetSize + packetNameSize;

            //	if (packetSerialiser.CanReadOpcode) bytesRead += 4;

            //	if (packetSerialiser.CanReadLength) bytesRead += 4;

            //	if (isTcp)
            //		serverInformation.ProcessedTcpPackets++;
            //	else
            //		serverInformation.ProcessedUdpPackets++;
            //}
        }

        private void Process(ISender sender, bool isTcp)
        {
            while (bytePool.CheckExistData(out int dataLength))
            {
                PacketContext context = packetContextObjectPool.Pop();
                context.Sender = sender;
                context.PacketLength = dataLength;
                if (context.PacketBytes == null || context.PacketBytes.Length < dataLength)
                    context.PacketBytes = new byte[dataLength + m_packetSendPool.PartSize];
                if (bytePool.CollectData(context.PacketBytes, dataLength))
                {
                    var opcode = BitConverter.ToUInt16(context.PacketBytes, bytePool.HeadSize);
                    //UnityEngine.Debug.LogError("解析出协议:" + opcode);
                    if (packetHandlers.GetPacketHandlers().TryGetValue(opcode, out var packetHandler))
                    {
                        //packetHandler.Handle(context).GetAwaiter().GetResult();
                        packetHandler.Handle(context,false);
                    }

                    if (isTcp)
                        serverInformation.ProcessedTcpPackets++;
                    else
                        serverInformation.ProcessedUdpPackets++;
                }
                else
                {
                    if (isTcp)
                        serverInformation.InvalidTcpPackets++;
                    else
                        serverInformation.InvalidUdpPackets++;
                }
                //packetContextObjectPool.Push(context);
            }
        }

        public void ProcessTcp(SocketAsyncEventArgs socketEvent)
        {
            var sender = tcpSenderObjectPool.Pop() as TcpSender;
            try
            {
                sender.Socket = ((AsyncUserToken)socketEvent.UserToken).Socket;
                ProcessPacketsFromSocketEventArgs(sender, socketEvent);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }

            tcpSenderObjectPool.Push(sender);
        }

        public void ProcessUdp(SocketAsyncEventArgs socketEvent)
        {
            var sender = udpSenderObjectPool.Pop() as UdpSender;
            try
            {
                sender.RemoteEndpoint = socketEvent.RemoteEndPoint as IPEndPoint;
                ProcessPacketsFromSocketEventArgs(sender, socketEvent, false);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }

            udpSenderObjectPool.Push(sender);
        }

        public void ProcessUdpFromBuffer(EndPoint endPoint, byte[] buffer, int offset = 0, int length = 0)
        {
            var sender = udpSenderObjectPool.Pop() as UdpSender;
            try
            {
                sender.RemoteEndpoint = endPoint as IPEndPoint;

                ProcessFromBuffer(sender, buffer, offset, length, false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }

            udpSenderObjectPool.Push(sender);
        }

        public void SetUdpSender(IUdpSocketSender sender)
        {
            _socketSender = sender;
            udpSenderObjectPool.SetFunc(() => new UdpSender(packetSerialiser, _socketSender, m_packetSendPool));
        }

        private void ProcessPacketsFromSocketEventArgs(ISender sender,
            SocketAsyncEventArgs eventArgs,
            bool isTcp = true)
        {
            ProcessFromBuffer(sender,
                    eventArgs.Buffer,
                    eventArgs.Offset,
                    eventArgs.BytesTransferred,
                    isTcp)
                .GetAwaiter()
                .GetResult();
        }
    }
}