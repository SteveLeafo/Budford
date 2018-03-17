using Budford.Model;
using SharpPresence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Control
{
    internal class DiscordRichPresence
    {
        static SharpPresence.Discord.EventHandlers handlers = new SharpPresence.Discord.EventHandlers();
        static SharpPresence.Discord.RichPresence presence = new SharpPresence.Discord.RichPresence();

        static internal void Initialize()
        {
            Discord.Initialize("424321820498329610", handlers);
        }

        static internal void ShutDown()
        {
            Discord.Shutdown();
        }

        static internal void StartGame(GameInformation game)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(game.Name);
            string fileName = Tools.HashGenerator.GenerateHashFromRpxRawData(bytes, bytes.Length).ToString("x8");

            if (!LargeImages.Contains(fileName))
            {
                fileName = "cemul";
            }

            presence.details = game.Name;
            presence.largeImageKey = fileName;
            presence.smallImageKey = "cemus";
            presence.state = "In Game";
            presence.partySize = 0;
            presence.partyMax = 0;
            presence.instance = 0;
            presence.startTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            Discord.UpdatePresence(presence);
        }

        static internal void EndGame()
        {
            presence.details = "Bewteen Games";
            presence.largeImageKey = "cemul";
            presence.smallImageKey = "cemus";
            presence.state = "Using Budford";
            presence.startTimestamp = 0;
            Discord.UpdatePresence(presence);
        }

        internal static readonly HashSet<string> LargeImages = new HashSet<string>(new[]
        {
            "2a676ad8",
            "2879efae",
            "3eb34661",
            "46519afb",
            "e0d803af",
            "8e5c931e",
            "8466bf85",
            "4551b8c7",
            "546e3303",
            "1c519bd9",
            "f6e74980",
            "2ced50d6",
            "b396714f",
            "992d9507",
            "ab0089cb",
            "c0c93da6",
            "6558f54d",
            "750c713b",
            "e4eb229f",
            "f528c80b",
            "17348878",
            "bc20d6f3",
            "6dcc90be",
            "bf76eb89",
            "1a33e3e8",
            "6f897a64",
            "dadaeadc",
            "38c2c8f3",
            "b5721cbc",
            "4e8d8fa2",
            "0235a8ea",
            "303e5cee",
            "10a5b0d5",
            "08f82564",
            "3de6df9d",
            "c8a08f9e",
            "21e85cb0",
            "25d574c8",
            "689815de",
            "64f816d8",
            "a421e299",
            "8d6c4407",
            "e727df39",
            "dab9960a",
            "7508c998",
            "ca1f0f57",
            "4c1ecbae",
            "65b5e43c",
            "e6d43996",
            "b4e7fda3",
            "9ff8997f",
            "48e94720",
            "b382ccca",
            "ca830d8f",
            "2dbaba72",
            "90e03af8",
            "16dd0604",
            "1f26dfd0",
            "732e18ca",
            "6dccaf61",
            "2e519ff8",
            "996d4ce7",
            "ad8873d8",
            "671d50c5",
            "ee9bb52d",
            "dc4243d3",
            "dc2a7900",
            "ba9156f3",
            "f53386e4",
            "1e89bcc5",
            "cb912e82",
            "2ed9208d",
            "1a9884bf",
            "6a57c325",
            "95f105e6",
            "ac49f5f3",
            "90d77e39",
            "af4eeb07",
            "d59af523",
            "6d585d16",
            "35529fea",
            "5c49b4d2",
            "4e8d1e97",
            "51b2f323",
            "f89ba915",
            "0289fa74",
            "fb75d914",
            "58ffa909",
            "d808c9e8",
            "bced619b",
            "2a42806c",
            "f21af64d",
            "610080f1",
            "7889d45e",
            "af5f884a",
            "7331b678",
            "9935c827",
            "ab1fee3c",
            "992b1fd1",
            "3f38661a",
            "9791215b",
            "93c60e28",
            "90375693",
            "997aff91",
            "4cae7ec7",
            "88ba42dd",
            "56df430e",
            "a3661c7d",
            "10665485",
            "bd6fbda8",
            "e095b2ef",
            "3148a4a4",
            "8c7b6d37",
            "99281e91",
            "463092f9",
            "c07a2697",
            "5d63fe9f",
            "cb891905",
            "0629370d",
            "fcfe3a03",
            "bd4d9634",
            "9970ccec",
            "39c99022",
            "c7197d2d",
            "2385a6ae",
            "c97947f4",
            "5fbb2f19",
            "17fc5ac4",
            "f61cca65",
            "7f603706",
            "2744ccf6",
            "0bd2a4f9",
            "35926cce",
            "1631edb0",
            "ad1c9c66",
            "a317ca1d",
            "f76aa68c",
            "becf0fba",
            "fab264e7"
        });

    }
}
