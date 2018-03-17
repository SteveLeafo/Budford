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

        static internal void Update(GameInformation game)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(game.Name);
            string fileName = Tools.HashGenerator.GenerateHashFromRpxRawData(bytes, bytes.Length).ToString("x8");

            if (!Discord.LargeImages.Contains(fileName))
            {
                fileName = "cemul";
            }

            Discord.Initialize("424321820498329610", handlers);

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
            Discord.Shutdown();
        }
    }
}
