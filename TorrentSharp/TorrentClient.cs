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
        internal BencodeParser BencodeParser;
        public string PeerId { get; set; }
        public string Ip { get; set; }
        public int Port { get; private set; } = -1;
        private readonly IDictionary<string, Tracker> _trackers;
        public IReadOnlyCollection<Tracker> Trackers => _trackers.Values.AsReadOnly();
        public bool CompactByDefault { get; } = true;

        public TorrentClient()
        {
            BencodeParser = new BencodeParser();
            _trackers = new ConcurrentDictionary<string, Tracker>();
        }

        public void Start(int port)
        {
            Port = port;
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