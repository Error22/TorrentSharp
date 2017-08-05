using System.Net.Http;
using System.Threading.Tasks;
using BencodeNET.Objects;

namespace TorrentSharp
{
    public class Tracker
    {
        private readonly TorrentClient _client;
        private readonly HttpClient _httpClient;
        public string AnnounceUrl { get; }
        public bool Compact { get; set; }
        public string TrackerId { get; set; }

        internal Tracker(TorrentClient client, string announceUrl)
        {
            _client = client;
            Compact = _client.CompactByDefault;
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
            UriHelper.AddParam(ref url, "compact", Compact ? "1" : "0");
            UriHelper.AddParam(ref url, "event", "started");
            if (TrackerId != null)
                UriHelper.AddParam(ref url, "trackerid", TrackerId);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage rawResponse = Task.Run(() => _httpClient.SendAsync(request)).Result;
            byte[] rawBytes = Task.Run(() => rawResponse.Content.ReadAsByteArrayAsync()).Result;

            TrackerResponse response = new TrackerResponse((BDictionary) _client.BencodeParser.Parse(rawBytes));
            if (response.TrackerId != null)
                TrackerId = response.TrackerId;
            return response;
        }
    }
}