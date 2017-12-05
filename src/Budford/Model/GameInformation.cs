﻿using System;
namespace Budford.Model
{
    public class GameInformation
    {
        public string Name  = "";
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
        public string Type = "";
        public int Rating = 1;

        public DateTime LastPlayed = DateTime.MinValue;
        public int PlayCount = 0;
        public int GraphicsPacksCount = 0;
        public GameSettings GameSetting;
        internal bool Exists = false;
    }
}
