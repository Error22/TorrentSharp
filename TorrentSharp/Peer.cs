using System;
using System.IO;
using System.Linq;
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

        public Peer(TorrentClient client, Torrent torrent, PeerInfo info)
        {
            _initiator = true;
            _client = client;
            _torrent = torrent;

            Console.WriteLine($"Connecting to {info.Ip}:{info.Port}");

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
            char[] ident = _reader.ReadChars(_reader.ReadByte());
            _reader.ReadBytes(8);
            byte[] hash = _reader.ReadBytes(20);
            byte[] id = _reader.ReadBytes(20);

            Console.WriteLine(
                $"Ident: {new string(ident)} ({ident.SequenceEqual(ProtocolIdentifier.ToCharArray())})");
            Console.WriteLine(
                $"Hash: {Encoding.UTF8.GetString(hash)} ({hash.SequenceEqual(_torrent.InfoHashBytes)})");
            Console.WriteLine($"Ident: {Encoding.UTF8.GetString(id)}");

            while (true)
            {
                Console.WriteLine($"READ: " + _reader.ReadByte());
            }
        }
    }
}