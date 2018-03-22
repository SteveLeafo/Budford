using System;
using System.Runtime.InteropServices;

namespace Budford.Utilities
{
    internal class Discord
    {
        public struct EventHandlers
        {
            public IntPtr Ready;
            public IntPtr Disconnected;
            public IntPtr Errored;
            public IntPtr JoinGame;
            public IntPtr SpectateGame;
            public IntPtr JoinRequest;
        }

        //--------------------------------------------------------------------------------

        public struct RichPresence
        {
            public string State;
            public string Details;
            public Int64 StartTimestamp;
            public Int64 EndTimestamp;
            public string LargeImageKey;
            public string LargeImageText;
            public string SmallImageKey;
            public string SmallImageText;
            public string PartyId;
            public int PartySize;
            public int PartyMax;
            public string MatchSecret;
            public string JoinSecret;
            public string SpectateSecret;
            public sbyte Instance;
        }

        //--------------------------------------------------------------------------------

        public enum Reply
        {
            No = 0,
            Yes = 1,
            Ignore = 2
        }

        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_Initialize([MarshalAs(UnmanagedType.LPStr)]string applicationId,
            ref EventHandlers handlers,
            int autoRegister,
            [MarshalAs(UnmanagedType.LPStr)]string optionalSteamId);

        public static void Initialize(string appId, EventHandlers handlers)
        {
            Discord_Initialize(appId, ref handlers, 1, String.Empty);
        }

        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_UpdatePresence(IntPtr presence);

        public static void UpdatePresence(RichPresence presence)
        {
            IntPtr ptrPresence = Marshal.AllocHGlobal(Marshal.SizeOf(presence));
            Marshal.StructureToPtr(presence, ptrPresence, false);
            Discord_UpdatePresence(ptrPresence);
        }

        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_Shutdown();

        public static void Shutdown()
        {
            Discord_Shutdown();
        }

        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_UpdateConnection();

        public static void UpdateConnection()
        {
            Discord_UpdateConnection();
        }

        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_RunCallbacks();

        public static void RunCallbacks()
        {
            Discord_RunCallbacks();
        }
        //--------------------------------------------------------------------------------

        [DllImport("discord-rpc.dll")]
        private static extern void Discord_Respond(string userId, int reply);

        public static void Respond(string userId, Reply reply)
        {
            Discord_Respond(userId, (int)reply);
        }
    }
}
