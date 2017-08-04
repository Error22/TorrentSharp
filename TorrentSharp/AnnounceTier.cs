using System.Collections.Generic;

namespace TorrentSharp
{
    public class AnnounceTier
    {
        public IList<Tracker> Trackers { get; }

        internal AnnounceTier(IEnumerable<string> trackers)
        {
            Trackers = new List<Tracker>();
            foreach (string tracker in trackers)
                Trackers.Add(new Tracker(tracker));
        }
    }
}