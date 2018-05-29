using Budford.Model;

namespace Budford.Control
{
    internal static class GameFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool FilterCheckedOut(Model.Model model, GameInformation game)
        {
            if (CheckPlatformFilter(model, game))
            {
                if (CheckRegionFilter(model, game))
                {
                    if (CheckStatusFilter(model, game))
                    {
                        if (CheckOfficialStatusFilter(model, game))
                        {
                            if (CheckRatingFilter(model, game))
                            {
                                return CheckTypeFilter(model, game);
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckRegionFilter(Model.Model model, GameInformation game)
        {
            switch (game.Region)
            {
                case "USA":
                    if (!model.Filters.ViewRegionUsa) return false;
                    break;
                case "EUR":
                    if (!model.Filters.ViewRegionEur) return false;
                    break;
                case "JAP":
                    if (!model.Filters.ViewRegionJap) return false;
                    break;
                case "ALL":
                    if (!model.Filters.ViewRegionAll) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckStatusFilter(Model.Model model, GameInformation game)
        {
            switch (game.GameSetting.EmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!model.Filters.ViewStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!model.Filters.ViewStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!model.Filters.ViewStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!model.Filters.ViewStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!model.Filters.ViewStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!model.Filters.ViewStatusUnplayable) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckTypeFilter(Model.Model model, GameInformation game)
        {
            switch (game.Type)
            {
                case "WiiU":
                    if (!model.Filters.ViewTypeWiiU) return false;
                    break;
                case "eShop":
                    if (!model.Filters.ViewTypeEshop) return false;
                    break;
                case "Channel":
                    if (!model.Filters.ViewTypeChannel) return false;
                    break;
                default:
                    if (!model.Filters.ViewTypeVc) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckPlatformFilter(Model.Model model, GameInformation game)
        {
            if (game.LaunchFileName == "WiiULauncher.rpx")
            {
                if (game.Name.ToUpper().Contains("SONIC"))
                {
                    return model.Filters.ViewPlatformWiiU;
                }
                return model.Filters.ViewPlatformHtml5;
            }
            else if (game.LaunchFileName == "VESSEL.rpx")
            {
                return model.Filters.ViewPlatformN64;
            }
            else if (game.LaunchFileName.StartsWith("WUP-F"))
            {
                return model.Filters.ViewPlatformNes;
            }
            else if (game.LaunchFileName.StartsWith("WUP-J"))
            {
                return model.Filters.ViewPlatformSnes;
            }
            else if (game.LaunchFileName == "hachihachi_ntr.rpx")
            {
                return model.Filters.ViewPlatformDs;
            }
            else
            {
                return model.Filters.ViewPlatformWiiU;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckRatingFilter(Model.Model model, GameInformation game)
        {
            switch (game.Rating)
            {
                case 5:
                    if (!model.Filters.ViewRating5) return false;
                    break;
                case 4:
                    if (!model.Filters.ViewRating4) return false;
                    break;
                case 3:
                    if (!model.Filters.ViewRating3) return false;
                    break;
                case 2:
                    if (!model.Filters.ViewRating2) return false;
                    break;
                case 1:
                    if (!model.Filters.ViewRating1) return false;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool CheckOfficialStatusFilter(Model.Model model, GameInformation game)
        {
            switch (game.GameSetting.OfficialEmulationState)
            {
                case GameSettings.EmulationStateType.NotSet:
                    if (!model.Filters.ViewOfficialStatusNotSet) return false;
                    break;
                case GameSettings.EmulationStateType.Perfect:
                    if (!model.Filters.ViewOfficialStatusPerfect) return false;
                    break;
                case GameSettings.EmulationStateType.Playable:
                    if (!model.Filters.ViewOfficialStatusPlayable) return false;
                    break;
                case GameSettings.EmulationStateType.Runs:
                    if (!model.Filters.ViewOfficialStatusRuns) return false;
                    break;
                case GameSettings.EmulationStateType.Loads:
                    if (!model.Filters.ViewOfficialStatusLoads) return false;
                    break;
                case GameSettings.EmulationStateType.Unplayable:
                    if (!model.Filters.ViewOfficialStatusUnplayable) return false;
                    break;
            }
            return true;
        }       
    }
}
