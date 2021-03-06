﻿using System.Collections.Generic;
using BencodeNET.Objects;

namespace TorrentSharp.Trackers
{
    public class TrackerResponse
    {
        public bool Success => FailureReason == null;
        public string FailureReason { get; }
        public string WarningMessage { get; }
        public int? Interval { get; }
        public int? MinInterval { get; }
        public string TrackerId { get; }
        public int? Complete { get; }
        public int? InComplete { get; }
        public IReadOnlyList<PeerInfo> Peers { get; }

        internal TrackerResponse(string failureReason)
        {
            FailureReason = failureReason;
        }

        internal TrackerResponse(BDictionary dictionary)
        {
            FailureReason = ((BString) dictionary["failure reason"])?.ToString();
            WarningMessage = ((BString) dictionary["warning message"])?.ToString();
            Interval = (int?) ((BNumber) dictionary["interval"])?.Value;
            MinInterval = (int?) ((BNumber) dictionary["min interval"])?.Value;
            TrackerId = ((BString) dictionary["tracker id"])?.ToString();
            Complete = (int?) ((BNumber) dictionary["complete"])?.Value;
            InComplete = (int?) ((BNumber) dictionary["incomplete"])?.Value;

            IBObject peersObject = dictionary["peers"];
            if (peersObject == null)
                return;

            List<PeerInfo> peers = new List<PeerInfo>();
            Peers = peers;

            if (peersObject is BString)
            {
                IReadOnlyList<byte> data = ((BString) peersObject).Value;

                for (int i = 0; i < data.Count; i += 6)
                {
                    string ip = $"{data[i]}.{data[i + 1]}.{data[i + 2]}.{data[i + 3]}";
                    int port = (data[i + 4] << 8) + data[i + 5];
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