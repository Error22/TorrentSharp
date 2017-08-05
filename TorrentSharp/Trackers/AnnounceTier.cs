using System.Collections.Generic;

namespace TorrentSharp.Trackers
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