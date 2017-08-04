namespace TorrentSharp.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            TorrentClient client = new TorrentClient
            {
                PeerId = "-CS1000-000000000000"
            };
            client.Start(6881);
        }
    }
}