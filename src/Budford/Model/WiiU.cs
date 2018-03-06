using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Model
{
    static class WiiU
    {
        internal static readonly string[][] Dumps =
        {
            // File Name, Budford Copy, Cemu Copy, Source

            // Sound
            new[]{"snd_user.rpl", "cafeLibs", "cafeLibs", "http://compat.cemu.info/wiki/System_RPL_Files", "1115"},
            new[]{"snduser2.rpl", "cafeLibs", "cafeLibs", "http://compat.cemu.info/wiki/System_RPL_Files", "1115"},

            // Online
            new[]{"seeprom.bin", "cafeLibs", "", "https://www.reddit.com/r/cemu/comments/7bic6u/a_complete_tutorial_to_playing_online_on_cemu/", "1110"},
            new[]{"otp.bin", "cafeLibs", "", "https://www.reddit.com/r/cemu/comments/7bic6u/a_complete_tutorial_to_playing_online_on_cemu/", "1110"},
            new[]{"account.dat", "cafeLibs", "mlc01\\usr\\save\\system\\act\\80000001", "https://www.reddit.com/r/cemu/comments/7bic6u/a_complete_tutorial_to_playing_online_on_cemu/", "1110"},
            new[]{"WIIU*", "cafeLibs\\ccerts", "mlc01\\sys\\title\\0005001b\\10054000\\content\\ccerts", "https://www.reddit.com/r/cemu/comments/7bic6u/a_complete_tutorial_to_playing_online_on_cemu/", "1110"},
            new[]{"*.der", "cafeLibs\\scerts", "mlc01\\sys\\title\\0005001b\\10054000\\content\\scerts", "https://www.reddit.com/r/cemu/comments/7bic6u/a_complete_tutorial_to_playing_online_on_cemu/", "1110"},

            // Fonts
            new[]{"CafeCn.ttf", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10042400\\content", "", "1114"},
            new[]{"CafeKr.ttf", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10042400\\content", "", "1114"},
            new[]{"CafeStd.ttf", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10042400\\content", "", "1114"},
            new[]{"CafeTw.ttf", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10042400\\content", "", "1114"},

            // NSMB
            new[]{"FFLResHigh.dat", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10056000\\content", "", "0"},
            new[]{"FFLResHighLG.dat", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10056000\\content", "", "0"},
            new[]{"FFLResMiddle.dat", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10056000\\content", "", "0"},
            new[]{"FFLResMiddleLG.dat", "cafeLibs", "mlc01\\sys\\title\\0005001b\\10056000\\content", "", "0"}
        };
    }
}
