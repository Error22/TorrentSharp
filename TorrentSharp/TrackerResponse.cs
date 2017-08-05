using System;
using System.Collections.Generic;
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
        public IReadOnlyList<PeerInfo> Peers { get; }

        internal TrackerResponse(BDictionary dictionary)
        {
            FailureReason = ((BString) dictionary["failure reason"])?.ToString();
            WarningMessage = ((BString) dictionary["warning message"])?.ToString();
            Interval = (int) (((BNumber) dictionary["interval"])?.Value ?? -1);
            MinInterval = (int) (((BNumber) dictionary["min interval"])?.Value ?? -1);
            TrackerId = ((BString) dictionary["tracker id"])?.ToString();
            Complete = (int) (((BNumber) dictionary["complete"])?.Value ?? -1);
            InComplete = (int) (((BNumber) dictionary["incomplete"])?.Value ?? -1);

            IBObject peersObject = dictionary["peers"];
            List<PeerInfo> peers = new List<PeerInfo>();
            Peers = peers;
            
            if (peersObject is BString)
            {
                IReadOnlyList<byte> data = ((BString) peersObject).Value;

                for (int i = 0; i < data.Count; i += 6)
                {
                    string ip = $"{data[i]}.{data[i + 1]}.{data[i + 2]}.{data[i + 3]}";
                    int port = (data[i + 5] << 8) + data[i + 4];
                    peers.Add(new PeerInfo(null, ip, port));
                }
            }
            else
            {
                BList<BDictionary> data = (BList<BDictionary>) peersObject;
                foreach (BDictionary obj in data)
                {
                    string id = ((BString) obj["peer id"]).ToString();
                    string ip = ((BString) obj["ip"]).ToString();
                    int port = (int) ((BNumber) obj["port"]).Value;
                    peers.Add(new PeerInfo(id, ip, port));
                }
            }
        }
    }
}