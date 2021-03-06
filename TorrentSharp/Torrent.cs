﻿using System.Collections.Generic;
using System.Linq;
using TorrentSharp.Trackers;
using BTorrent = BencodeNET.Torrents.Torrent;

namespace TorrentSharp
{
    public class Torrent
    {
        private readonly TorrentClient _client;
        private readonly BTorrent _btorrent;
        public IList<AnnounceTier> AnnounceTiers { get; }

        public long TotalSize => _btorrent.TotalSize;
        public byte[] InfoHashBytes => _btorrent.OriginalInfoHashBytes;

        internal Torrent(TorrentClient client, BTorrent btorrent)
        {
            _client = client;
            _btorrent = btorrent;
            AnnounceTiers = new List<AnnounceTier>();
            foreach (IList<string> trackers in btorrent.Trackers)
                AnnounceTiers.Add(new AnnounceTier(this, trackers.Select(client.GetOrCreateTracker).ToList()));
        }

        public TrackerResponse Announce()
        {
            foreach (AnnounceTier announceTier in AnnounceTiers)
            {
                TrackerResponse response = announceTier.Announce();
                if (response.Success)
                    return response;
            }
            return new TrackerResponse("Torrent Error: No trackers responded");
        }
    }
}