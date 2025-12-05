using System;
using System.Linq;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Validation helper using configuration from ConfigManager
    /// </summary>
    public static class ValidationHelper
    {
        private static ConfigManager Config => ConfigManager.Instance;

        // Specialty validation using configuration
        public static bool IsValidSpecialty(string specialty)
        {
            return Config.ValidSpecialties.Contains(specialty);
        }

        // Category validation using configuration
        public static bool IsValidCategory(string category)
        {
            return Config.ValidCategories.Contains(category);
        }

        // Skill level validation using configuration
        public static bool IsValidSkillLevel(int skillLevel)
        {
            return skillLevel >= Config.SkillLevelRange.Min && skillLevel <= Config.SkillLevelRange.Max;
        }

        // Loyalty validation using configuration
        public static bool IsValidLoyalty(int loyalty)
        {
            return loyalty >= Config.LoyaltyScoreRange.Min && loyalty <= Config.LoyaltyScoreRange.Max;
        }

        // Condition validation using configuration
        public static bool IsValidCondition(int condition)
        {
            return condition >= Config.ConditionRange.Min && condition <= Config.ConditionRange.Max;
        }

        // Diabolical rating validation using configuration
        public static bool IsValidDiabolicalRating(int rating)
        {
            return rating >= Config.DiabolicalRatingRange.Min && rating <= Config.DiabolicalRatingRange.Max;
        }

        // Security level validation using configuration
        public static bool IsValidSecurityLevel(int level)
        {
            return level >= Config.SecurityLevelRange.Min && level <= Config.SecurityLevelRange.Max;
        }
        
        // Mood status validation using configuration
        public static bool IsValidMoodStatus(string mood)
        {
            return Config.ValidMoodStatuses.Contains(mood);
        }

        // Scheme status validation using configuration
        public static bool IsValidSchemeStatus(string status)
        {
            return Config.ValidSchemeStatuses.Contains(status);
        }
    }
}
