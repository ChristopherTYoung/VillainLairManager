using System;
using VillainLairManager.Utils;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Minion model with business logic mixed in (anti-pattern)
    /// </summary>
    public class Minion
    {
        public int MinionId { get; set; }
        public string Name { get; set; }
        public int SkillLevel { get; set; }
        public string Specialty { get; set; }
        public int LoyaltyScore { get; set; }
        public decimal SalaryDemand { get; set; }
        public int? CurrentBaseId { get; set; }
        public int? CurrentSchemeId { get; set; }
        public string MoodStatus { get; set; }
        public DateTime LastMoodUpdate { get; set; }

        // Business logic mixed into model (anti-pattern)
        public void UpdateMood()
        {
            var config = ConfigManager.Instance;
            // Business rules embedded in model
            if (this.LoyaltyScore > config.HighLoyaltyThreshold)
                this.MoodStatus = config.MoodHappy;
            else if (this.LoyaltyScore < config.LowLoyaltyThreshold)
                this.MoodStatus = config.MoodBetrayal;
            else
                this.MoodStatus = config.MoodGrumpy;

            this.LastMoodUpdate = DateTime.Now;

            // Directly accesses database (anti-pattern)
            DatabaseHelper.UpdateMinion(this);
        }

        // Static utility method in model (anti-pattern)
        public static bool IsValidSpecialty(string specialty)
        {
            // Use ValidationHelper instead of duplicating logic
            return ValidationHelper.IsValidSpecialty(specialty);
        }

        // Business logic for loyalty calculation
        public void UpdateLoyalty(decimal actualSalaryPaid)
        {
            var config = ConfigManager.Instance;
            if (actualSalaryPaid >= this.SalaryDemand)
            {
                this.LoyaltyScore += config.LoyaltyGrowthRate;
            }
            else
            {
                this.LoyaltyScore -= config.LoyaltyDecayRate;
            }

            // Clamp to valid range
            if (this.LoyaltyScore > config.LoyaltyScoreRange.Max) this.LoyaltyScore = config.LoyaltyScoreRange.Max;
            if (this.LoyaltyScore < config.LoyaltyScoreRange.Min) this.LoyaltyScore = config.LoyaltyScoreRange.Min;

            // Update mood based on new loyalty
            UpdateMood();
        }

        // ToString for ComboBox display
        public override string ToString()
        {
            return $"{Name} ({Specialty}, Skill: {SkillLevel})";
        }
    }
}
