﻿using System;
using System.Collections.Generic;
namespace Budford.Model
{
    public class GameInformation
    {
        public string Name  = "";
        internal string Schmooker = "";
        public string Region  = "";
        public string Publisher  = "";
        public string ProductCode  = "";
        public string CompanyCode  = "";
        public string LaunchFile  = "";
        public string LaunchFileName  = "";
        public string Comments = "";
        public string Size  = "";
        public string SaveDir  = "??      ";
        public string TitleId  = "";
        public string GroupId  = "";
        public int TitleVersion = int.MaxValue;
        public string Type = "";
        internal string UnplayableReason = "Playable";
        public int Rating = 1;
        public long PlayTime = 0;

        internal int ShaderCacheFileSize = -1;
        internal int ShaderCacheCount = -1;

        public DateTime LastPlayed = DateTime.MinValue;
        public int PlayCount = 0;
        public int GraphicsPacksCount = 0;
        public GameSettings GameSetting;
        public CemuHookSettings CemuHookSetting = new CemuHookSettings();
        public List<StatusUpdate> StatusUpdates = new List<StatusUpdate>();
        internal bool Exists = false;
        public bool Image;
        public string RpxFile;
        internal string CleanName;
    }
}
