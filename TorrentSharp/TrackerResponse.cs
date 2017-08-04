using BencodeNET.Objects;

namespace TorrentSharp
{
    public class TrackerResponse
    {
        public string FailureReason { get; }
        public string WarningMessage { get; }
        public int Interval { get; }
        public int MinInterval { get; }
        public string TrackerId { get; }
        public int Complete { get; }
        public int InComplete { get; }
        
        internal TrackerResponse(BDictionary dictionary)
        {
            FailureReason = ((BString) dictionary["failure reason"])?.ToString();
            WarningMessage = ((BString) dictionary["warning message"])?.ToString();
            Interval = (int) (((BNumber) dictionary["interval"])?.Value ?? -1);
            MinInterval = (int) (((BNumber) dictionary["min interval"])?.Value ?? -1);
            TrackerId = ((BString) dictionary["tracker id"])?.ToString();
            Complete = (int) (((BNumber) dictionary["complete"])?.Value ?? -1);
            InComplete = (int) (((BNumber) dictionary["incomplete"])?.Value ?? -1);
        }
    }
}