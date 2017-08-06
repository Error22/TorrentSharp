using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Be.IO;
using TorrentSharp.Trackers;

namespace TorrentSharp
{
    public class Peer
    {
        private const string ProtocolIdentifier = "BitTorrent protocol";
        private readonly bool _initiator;
        private readonly TorrentClient _client;
        private readonly Torrent _torrent;
        private readonly TcpClient _tcpClient;
        private readonly Thread _readThread;
        private readonly BeBinaryReader _reader;
        private readonly BeBinaryWriter _writer;

        public string PeerId { get; private set; }

        public Peer(TorrentClient client, Torrent torrent, PeerInfo info)
        {
            _initiator = true;
            _client = client;
            _torrent = torrent;

            Console.WriteLine($"[{info.Ip}:{info.Port}] Connecting");

            _tcpClient = new TcpClient(info.Ip, info.Port);
            NetworkStream stream = _tcpClient.GetStream();
            _reader = new BeBinaryReader(stream);
            _writer = new BeBinaryWriter(stream);

            _readThread = new Thread(ReadThreadBody);
            _readThread.Start();

            SendHandshake();
        }

        private void SendHandshake()
        {
            _writer.Write(ProtocolIdentifier);
            _writer.Write(new byte[8]);
            _writer.Write(_torrent.InfoHashBytes);
            if (!_initiator)
                _writer.Write(_client.PeerId.ToCharArray());
        }

        private void ReadThreadBody()
        {
            HandleHandshake();

            while (true)
            {
                int length = _reader.ReadInt32();
                if (length == 0)
                {
                    HandleKeepAlive();
                    continue;
                }

                byte[] data = _reader.ReadBytes(length);
                BeBinaryReader reader = new BeBinaryReader(new MemoryStream(data));
                int mid = reader.ReadByte();

                switch (mid)
                {
                    case 0:
                        HandleChoke(reader, true);
                        break;
                    case 1:
                        HandleChoke(reader, false);
                        break;
                    case 2:
                        HandleInterested(reader, true);
                        break;
                    case 3:
                        HandleInterested(reader, false);
                        break;
                    case 4:
                        HandleHave(reader);
                        break;
                    case 5:
                        HandleBitfield(reader);
                        break;
                    case 6:
                        HandleRequest(reader);
                        break;
                    case 7:
                        HandlePiece(reader, length);
                        break;
                    case 8:
                        HandleCancel(reader);
                        break;
                    case 9:
                        HandlePort(reader);
                        break;
                    default:
                        Debug($"Unknown packet! {mid}");
                        break;
                }

                reader.Close();
            }
        }

        private void HandleHandshake()
        {
            char[] protocolIdentifier = _reader.ReadChars(_reader.ReadByte());
            Debug($"Remote Protocol: {new string(protocolIdentifier)}");
            if (!ProtocolIdentifier.SequenceEqual(protocolIdentifier))
            {
                Debug("Invalid remote protocol");
                Disconnect();
                return;
            }

            byte[] extensionBytes = _reader.ReadBytes(8);
            if (!extensionBytes.SequenceEqual(new byte[8]))
                Debug("Remote client has extensions");

            byte[] torrentHash = _reader.ReadBytes(20);
            Debug($"Remote Torrent Hash: {Encoding.UTF8.GetString(torrentHash)}");
            if (_initiator)
            {
                if (!_torrent.InfoHashBytes.SequenceEqual(torrentHash))
                {
                    Debug("Invalid remote torrent hash");
                    Disconnect();
                    return;
                }
            }
            else
            {
                // TODO: Check we have this torrent
                throw new NotImplementedException("Unable to handle incoming requests");
            }

            if (!_initiator)
                SendHandshake();

            string id = Encoding.UTF8.GetString(_reader.ReadBytes(20));
            Debug($"Peer Id: {id}");
            // TODO: Check for banned peers
            PeerId = id;

            if (_initiator)
                _writer.Write(_client.PeerId.ToCharArray());
        }

        private void HandleKeepAlive()
        {
            Debug("Keep-Alive");
            // TODO: Log last keep alive
        }

        private void HandleChoke(BeBinaryReader reader, bool chocked)
        {
            Debug(chocked ? "Chocked" : "Unchocked");
        }

        private void HandleInterested(BeBinaryReader reader, bool interested)
        {
            Debug(interested ? "Interested" : "Uninterested");
        }

        private void HandleHave(BeBinaryReader reader)
        {
            Debug($"Have {reader.ReadInt32()}");
        }

        private void HandleBitfield(BeBinaryReader reader)
        {
            Debug("Bitfield");
        }

        private void HandleRequest(BeBinaryReader reader)
        {
            Debug($"Request - Index={reader.ReadInt32()} Begin={reader.ReadInt32()} Length={reader.ReadInt32()}");
        }

        private void HandlePiece(BeBinaryReader reader, int packetLength)
        {
            int length = packetLength - 9;
            Debug($"Piece - Index={reader.ReadInt32()} Begin={reader.ReadInt32()} Length={length}");
        }

        private void HandleCancel(BeBinaryReader reader)
        {
            Debug($"Cancel - Index={reader.ReadInt32()} Begin={reader.ReadInt32()} Length={reader.ReadInt32()}");
        }

        private void HandlePort(BeBinaryReader reader)
        {
            Debug($"Port {reader.ReadUInt16()}");
        }

        private void Disconnect()
        {
            Debug("Disconnect");
            _tcpClient.GetStream().Close();
            _tcpClient.Close();
        }

        private void Debug(string msg)
        {
            if (PeerId != null)
            {
                Console.WriteLine($"[{PeerId}] {msg}");
                return;
            }

            IPEndPoint ip = (IPEndPoint) _tcpClient.Client.RemoteEndPoint;
            Console.WriteLine($"[{ip.Address}:{ip.Port}] {msg}");
        }
    }
}