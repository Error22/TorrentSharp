using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BencodeNET.Parsing;
using TorrentSharp.Trackers;
using TorrentSharp.Utils;
using BTorrent = BencodeNET.Torrents.Torrent;

namespace TorrentSharp
{
    public class TorrentClient
    {
        private static readonly char[] IdChars =
            "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();

        internal readonly BencodeParser BencodeParser;
        public string PeerId { get; set; }
        public string ExternalIp { get; set; }
        public int ExternalPort { get; set; }
        public int InternalPort { get; private set; } = -1;
        private readonly IDictionary<string, Tracker> _trackers;
        public IReadOnlyCollection<Tracker> Trackers => _trackers.Values.AsReadOnly();
        public bool CompactByDefault { get; } = true;

        public TorrentClient()
        {
            BencodeParser = new BencodeParser();
            _trackers = new ConcurrentDictionary<string, Tracker>();

            Random random = new Random();
            char[] rand = new char[12];
            for (int i = 0; i < rand.Length; i++)
                rand[i] = IdChars[random.Next(IdChars.Length)];
            PeerId = $"-ts1000-{new string(rand)}";
        }

        public void Start(int port)
        {
            InternalPort = port;
        }

        internal Tracker GetOrCreateTracker(string announceUrl)
        {
            if (_trackers.ContainsKey(announceUrl))
                return _trackers[announceUrl];
            Tracker tracker = new Tracker(this, announceUrl);
            _trackers[announceUrl] = tracker;
            return tracker;
        }

        public Torrent LoadTorrentFile(string fileName)
        {
            return new Torrent(this, BencodeParser.Parse<BTorrent>(fileName));
        }
    }
}