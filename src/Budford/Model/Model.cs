using System.Collections.Generic;

namespace Budford.Model
{
    public class Model
    {
        // The currently active / selected game.
        public string CurrentId;

        // The currently active user.
        public string CurrentUser;

        // The installed games.
        internal Dictionary<string, GameInformation> GameData  = new Dictionary<string, GameInformation>();
        public List<GameInformation> GameData2  = new List<GameInformation>();

        internal Dictionary<string, List<GraphicsPack>> GraphicsPacks  = new Dictionary<string, List<GraphicsPack>>();

        // Currently available releases.
        public List<OldVersion> OldVersions  = new List<OldVersion>();

        // A list of users that have been added to the system.
        public List<User> Users  = new List<User>();

        // All of our configuration options.
        public Settings Settings  = new Settings();

        // Filters for the main list view
        public ViewFilters Filters  = new ViewFilters();

        internal List<string> Errors = new List<string>();
    }
}
