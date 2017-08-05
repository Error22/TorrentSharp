using System.Collections.Generic;

namespace TorrentSharp.Trackers
{
    public class AnnounceTier
    {
        private readonly Torrent _torrent;
        public IList<Tracker> Trackers { get; }

        internal AnnounceTier(Torrent torrent, IList<Tracker> trackers)
        {
            _torrent = torrent;
            Trackers = trackers;
        }

        public TrackerResponse Announce()
        {
            for (int i = 0; i < Trackers.Count; i++)
            {
                Tracker tracker = Trackers[i];
                TrackerResponse response = tracker.Announce(_torrent);

                if (!response.Success) continue;
                if (i == 0) return response;
                Trackers.RemoveAt(i);
                Trackers.Insert(0, tracker);
                return response;
            }
            return new TrackerResponse("Tier Error: No trackers responded");
        }
    }
}