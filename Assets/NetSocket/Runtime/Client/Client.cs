using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public class Client : IClient
    {
        private readonly ILogger logger;
        private readonly ClientBuilderOptions options;
        private readonly IClientPacketProcessor packetProcessor;
        private readonly IPacketSerialiser packetSerialiser;
        private readonly byte[] pingPacketBuffer;
        private readonly PingOptions pingOptions;
        private bool isRunning = true;
        private Socket tcpSocket;
        private UdpClient udpClient;
        private IPEndPoint udpEndpoint;
        private PacketSendPool packetSendPool;
        public EventHandler<Socket> Connected { get; set; }
        public EventHandler<Socket> Disconnected { get; set; }
        private Task m_tcpRecvTask;
        private Task m_udpRecvTask;

        public Client(ClientBuilderOptions options,
            IPacketSerialiser packetSerialiser,
            IClientPacketProcessor packetProcessor,
            ILogger logger)
        {
            this.options = options;
            this.packetSerialiser = packetSerialiser;
            this.packetProcessor = packetProcessor;
            this.logger = logger;
            this.pingPacketBuffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaa");
            this.pingOptions = new PingOptions(64, true);
            this.packetSendPool = new PacketSendPool(options.PacketPartSize);
        }

        /// <inheritdoc />
        public ConnectResult Connect()
        {
            if (this.options.TcpPort > 0 && this.tcpSocket == null)
            {
                try
                {
                    this.tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.tcpSocket.Connect(this.options.Ip, this.options.TcpPort);
                }
                catch (Exception exception)
                {
                    return new ConnectResult(exception.Message);
                }

                this.Connected?.Invoke(this, this.tcpSocket);

                StartTcpSocketThread();
            }

            if (this.options.UdpPort > 0 && this.udpClient == null)
            {
                try
                {
                    var address = IPAddress.Parse(this.options.Ip);
                    this.udpClient = new UdpClient(this.options.UdpPortLocal);
                    this.udpEndpoint = new IPEndPoint(address, this.options.UdpPort);
                }
                catch (Exception exception)
                {
                    return new ConnectResult(exception.Message);
                }

                StartUdpSocketThread();
            }

            return new ConnectResult(true);
        }

        public void MainThreadRefresh()
        {
            if (this.packetProcessor != null)
                this.packetProcessor.MainThreadRefresh();
        }

        /// <inheritdoc />
        public long Ping(int timeout)
        {
            var pingSender = new Ping();
            var reply = pingSender.Send(this.options.Ip, timeout, this.pingPacketBuffer, this.pingOptions);

            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime;
            }
            this.logger.LogError($"Could not get ping {reply.Status}");
            return -1;
        }

        public void SendTcp<T>(ushort opcode, T packet)
        {
            var packetData = packetSerialiser.Serialize(packet);
            SendTcp(opcode, packetData);
        }

        /// <inheritdoc />
        public void SendTcp(ushort opcode, byte[] packet)
        {
            if (this.tcpSocket == null)
            {
                throw new Exception("TCP client has not been initialised. Have you called .Connect()?");
            }
            var totalLength = 6;
            if (packet != null)
                totalLength += packet.Length;
            var packetData = packetSendPool.GetEmptyPacket(totalLength);
            packetData.MakePacket(opcode, packet, 0, packet.Length);
            this.tcpSocket.Send(packetData.buffer, 0, packetData.length, SocketFlags.None);
            packetSendPool.SaveBackPacket(packetData);
        }

        public void SendUdp<T>(ushort opcode, T packet)
        {
            var packetData = packetSerialiser.Serialize(packet);
            SendUdp(opcode, packetData);
        }

        /// <inheritdoc />
        public void SendUdp(ushort opcode, byte[] packet)
        {
            if (this.udpClient == null)
            {
                throw new Exception("UDP client has not been initialised. Have you called .Connect()?");
            }
            var totalLength = 6;
            if (packet != null)
                totalLength += packet.Length;
            var packetData = packetSendPool.GetEmptyPacket(totalLength);
            packetData.MakePacket(opcode, packet, 0, packet.Length);
            this.udpClient.Send(packetData.buffer, packetData.length, this.udpEndpoint);
            packetSendPool.SaveBackPacket(packetData);
        }

        /// <inheritdoc />
        public void SendTcp(ushort opcode, byte[] packet, int offset, int length)
        {
            if (this.tcpSocket == null)
            {
                throw new Exception("TCP client has not been initialised. Have you called .Connect()?");
            }
            var totalLength = 6;
            if (packet != null)
                totalLength += packet.Length;
            var packetData = packetSendPool.GetEmptyPacket(totalLength);
            packetData.MakePacket(opcode, packet, offset, length);
            this.tcpSocket.Send(packetData.buffer, 0, packetData.length, SocketFlags.None);
            packetSendPool.SaveBackPacket(packetData);
        }

        /// <inheritdoc />
        public void SendUdp(ushort opcode, byte[] packet, int offset, int length)
        {
            if (this.udpClient == null)
            {
                throw new Exception("UDP client has not been initialised. Have you called .Connect()?");
            }
            var totalLength = 6;
            if (packet != null)
                totalLength += packet.Length;
            var packetData = packetSendPool.GetEmptyPacket(totalLength);
            packetData.MakePacket(opcode, packet, offset, length);
            this.udpClient.Send(packetData.buffer, packetData.length, this.udpEndpoint);
            packetSendPool.SaveBackPacket(packetData);
        }


        /// <inheritdoc />
        public void SendUdp(byte[] packetData, int length)
        {
            if (this.udpClient == null)
            {
                throw new Exception("UDP client has not been initialised. Have you called .Connect()?");
            }
            this.udpClient.Send(packetData, length, this.udpEndpoint);
        }

        /// <inheritdoc />
        public void Stop()
        {
            this.isRunning = false;
            if (tcpSocket != null)
                tcpSocket.Disconnect(true);
        }

        private void StartTcpSocketThread()
        {
            m_tcpRecvTask = Task.Factory.StartNew(() =>
            {
                while (this.isRunning)
                {
                    if (this.tcpSocket.Poll(10, SelectMode.SelectWrite))
                    {
                        this.packetProcessor.Process(this.tcpSocket);
                    }

                    if (!this.tcpSocket.Connected)
                    {
                        this.Disconnected?.Invoke(this, this.tcpSocket);
                        break;
                    }
                }

                if (this.tcpSocket.Connected)
                {
                    this.tcpSocket.Disconnect(false);
                    this.tcpSocket.Close();
                    this.Disconnected?.Invoke(this, this.tcpSocket);
                }

                this.tcpSocket = null;
            });
        }

        private void StartUdpSocketThread()
        {
            m_udpRecvTask = Task.Factory.StartNew(() =>
            {
                this.logger.LogInformation(
                    $"Connecting to UDP at {this.options.Ip}:{this.options.UdpPort}");

                while (this.isRunning)
                {
                    try
                    {
                        var data = this.udpClient.ReceiveAsync()
                                        .GetAwaiter()
                                        .GetResult();

                        this.packetProcessor.Process(data);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogException(ex);
                    }
                }

                this.udpClient = null;
            });
        }
    }
}