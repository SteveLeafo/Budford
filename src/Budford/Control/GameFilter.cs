using Budford.Model;
using Budford.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budford.Control
{
    internal static class GameFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool FilterCheckedOut(Model.Model Model, GameInformation game)
        {
            if (!Model.Settings.IncludeWiiULauncherRpx)
            {
                if (game.LaunchFile.Contains("WiiULauncher.rpx"))
                {
                    return false;
                }
            }

            if (CheckRegionFilter(Model, game))
            {
                if (CheckStatusFilter(Model, game))
                {
                    if (CheckOfficialStatusFilter(Model, game))
                    {
                        if (CheckRatingFilter(Model, game))
                        {
                            return CheckTypeFilter(Model, game);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckRegionFilter(Model.Model Model, GameInformation game)
        {
            switch (game.Region)
            {
                case "USA":
                    if (!Model.Filters.ViewRegionUsa) return false;
                    break;
                case "EUR":
                    if (!Model.Filters.ViewRegionEur) return false;
                    break;
                case "JAP":
                    if (!Model.Filters.ViewRegionJap) return false;
                    break;
                case "ALL":
                    if (!Model.Filters.ViewRegionAll) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckStatusFilter(Model.Model Model, GameInformation game)
        {
            switch (game.GameSetting.EmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!Model.Filters.ViewStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!Model.Filters.ViewStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!Model.Filters.ViewStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!Model.Filters.ViewStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!Model.Filters.ViewStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!Model.Filters.ViewStatusUnplayable) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckTypeFilter(Model.Model Model, GameInformation game)
        {
            switch (game.Type)
            {
                case "WiiU":
                    if (!Model.Filters.ViewTypeWiiU) return false;
                    break;
                case "eShop":
                    if (!Model.Filters.ViewTypeEshop) return false;
                    break;
                case "Channel":
                    if (!Model.Filters.ViewTypeChannel) return false;
                    break;
                default:
                    if (!Model.Filters.ViewTypeVc) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckRatingFilter(Model.Model Model, GameInformation game)
        {
            switch (game.Rating)
            {
                case 5:
                    if (!Model.Filters.ViewRating5) return false;
                    break;
                case 4:
                    if (!Model.Filters.ViewRating4) return false;
                    break;
                case 3:
                    if (!Model.Filters.ViewRating3) return false;
                    break;
                case 2:
                    if (!Model.Filters.ViewRating2) return false;
                    break;
                case 1:
                    if (!Model.Filters.ViewRating1) return false;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckOfficialStatusFilter(Model.Model Model, GameInformation game)
        {
            switch (game.GameSetting.OfficialEmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!Model.Filters.ViewOfficialStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!Model.Filters.ViewOfficialStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!Model.Filters.ViewOfficialStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!Model.Filters.ViewOfficialStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!Model.Filters.ViewOfficialStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!Model.Filters.ViewOfficialStatusUnplayable) return false;
                    break;
            }
            return true;
        }       
    }
}
