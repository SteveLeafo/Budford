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
        public List<GameInformation> GameData2  = new List<GameInformation>();

        // Currently available releases.
        public List<OldVersion> OldVersions  = new List<OldVersion>();

        // Additional game save "snap shots"
        public List<SnapShot> ShapShots = new List<SnapShot>();

        // A list of users that have been added to the system.
        public List<User> Users  = new List<User>();

        // All of our configuration options.
        public Settings Settings  = new Settings();

        // Filters for the main list view
        public ViewFilters Filters  = new ViewFilters();

        // A log of all the errors that have occurred
        internal List<string> Errors = new List<string>();

        // Lookup table for game data
        internal Dictionary<string, GameInformation> GameData = new Dictionary<string, GameInformation>();

        internal Dictionary<string, GameInformation> WiiUApps = new Dictionary<string, GameInformation>();

        // Lookup table for graphic packs
        internal Dictionary<string, List<GraphicsPack>> GraphicsPacks = new Dictionary<string, List<GraphicsPack>>();
    }
}
