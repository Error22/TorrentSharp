﻿using System.Net.Http;
using System.Threading.Tasks;

namespace TorrentSharp
{
    public class Tracker
    {
        private readonly TorrentClient _client;
        private readonly HttpClient _httpClient;
        public string AnnounceUrl { get; }

        internal Tracker(TorrentClient client, string announceUrl)
        {
            _client = client;
            AnnounceUrl = announceUrl;
            _httpClient = new HttpClient();
        }

        public TrackerResponse Announce(Torrent torrent)
        {
            string url = AnnounceUrl;
            UriHelper.AddParam(ref url, "info_hash", UriHelper.UrlEncode(torrent.InfoHashBytes));
            UriHelper.AddParam(ref url, "peer_id", _client.PeerId);
            if (_client.Ip != null)
                UriHelper.AddParam(ref url, "ip", _client.Ip);
            UriHelper.AddParam(ref url, "port", _client.Port.ToString());
            UriHelper.AddParam(ref url, "uploaded", "0");
            UriHelper.AddParam(ref url, "downloaded", "0");
            UriHelper.AddParam(ref url, "left", torrent.TotalSize.ToString());
            UriHelper.AddParam(ref url, "compact", "1");
            UriHelper.AddParam(ref url, "event", "started");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = Task.Run(() => _httpClient.SendAsync(request)).Result;
            byte[] rawBytes = Task.Run(() => response.Content.ReadAsByteArrayAsync()).Result;

            return null;
        }
    }
}