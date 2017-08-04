namespace TorrentSharp
{
    public class Tracker
    {
        public string AnnounceUrl { get; }

        internal Tracker(string announceUrl)
        {
            AnnounceUrl = announceUrl;
        }
    }
}