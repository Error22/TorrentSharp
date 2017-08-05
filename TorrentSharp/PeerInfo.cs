namespace TorrentSharp
{
    public class PeerInfo
    {
        public string Id { get; }
        public string Ip { get; }
        public int Port { get; }

        internal PeerInfo(string id, string ip, int port)
        {
            Id = id;
            Ip = ip;
            Port = port;
        }
    }
}