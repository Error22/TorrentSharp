using System;
using TorrentSharp.Trackers;

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

            Torrent torrent = client.LoadTorrentFile("ubuntu-17.04-desktop-amd64.iso.torrent");
            TrackerResponse response = torrent.Announce();

            Console.WriteLine($"Success: {response.Success}");

            Console.Read();
        }
    }
}