using System.Collections.Generic;
using BTorrent = BencodeNET.Torrents.Torrent;

namespace TorrentSharp
{
    public class Torrent
    {
        private BTorrent _btorrent;
        public IList<AnnounceTier> AnnounceTiers { get; }

        internal Torrent(BTorrent btorrent)
        {
            _btorrent = btorrent;
            AnnounceTiers = new List<AnnounceTier>();
            foreach (IList<string> trackers in btorrent.Trackers)
                AnnounceTiers.Add(new AnnounceTier(trackers));
        }
    }
}