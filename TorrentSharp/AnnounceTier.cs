using System.Collections.Generic;

namespace TorrentSharp
{
    public class AnnounceTier
    {
        public IList<Tracker> Trackers { get; }

        internal AnnounceTier(IList<Tracker> trackers)
        {
            Trackers = trackers;
        }
    }
}