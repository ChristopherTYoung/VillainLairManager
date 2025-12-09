using System;
using System.IO;
using System.Text.Json;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Configuration manager that loads settings from appsettings.json
    /// Provides centralized access to all configuration values
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager _instance;
        private static readonly object _lock = new object();
        private AppSettings _settings;

        private ConfigManager()
        {
            LoadConfiguration();
        }

        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private void LoadConfiguration()
        {
            string configPath = "appsettings.json";
            
            try
            {
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    // Use defaults if config file doesn't exist
                    _settings = GetDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load configuration from {configPath}. Using defaults. Error: {ex.Message}");
                _settings = GetDefaultSettings();
            }
        }

        private AppSettings GetDefaultSettings()
        {
            return new AppSettings
            {
                Database = new DatabaseSettings
                {
                    Path = "villainlair.db",
                    ConnectionTimeout = 30
                },
                BusinessRules = new BusinessRulesSettings
                {
                    MaxMinionsPerScheme = 10,
                    MaxEquipmentPerScheme = 5,
                    DefaultMinionSalary = 5000.0m,
                    LoyaltyDecayRate = 5,
                    LoyaltyGrowthRate = 3,
                    ConditionDegradationRate = 5,
                    MaintenanceCostPercentage = 0.15m,
                    DoomsdayMaintenanceCostPercentage = 0.30m
                },
                Thresholds = new ThresholdsSettings
                {
                    LowLoyaltyThreshold = 40,
                    HighLoyaltyThreshold = 70,
                    OverworkedDays = 60,
                    SpecialistSkillLevel = 8,
                    MinEquipmentCondition = 50,
                    BrokenEquipmentCondition = 20,
                    HighDiabolicalRating = 8,
                    SuccessLikelihoodHighThreshold = 70,
                    SuccessLikelihoodLowThreshold = 30
                },
                ValidationRanges = new ValidationRangesSettings
                {
                    SkillLevel = new RangeSettings { Min = 1, Max = 10 },
                    LoyaltyScore = new RangeSettings { Min = 0, Max = 100 },
                    Condition = new RangeSettings { Min = 0, Max = 100 },
                    DiabolicalRating = new RangeSettings { Min = 1, Max = 10 },
                    SecurityLevel = new RangeSettings { Min = 1, Max = 10 },
                    SuccessLikelihood = new RangeSettings { Min = 0, Max = 100 }
                },
                ValidValues = new ValidValuesSettings
                {
                    MinionSpecialties = new[] { "Henchman", "Scientist", "Technician", "Hacking", "Explosives", "Disguise", "Combat", "Engineering", "Piloting" },
                    MoodStatuses = new[] { "Happy", "Grumpy", "Plotting Betrayal", "Exhausted" },
                    SchemeStatuses = new[] { "Planning", "Active", "On Hold", "Completed", "Failed" },
                    EquipmentCategories = new[] { "Weapon", "Vehicle", "Gadget", "Doomsday Device", "Surveillance", "Transportation", "Communication", "Weapons" }
                },
                DefaultValues = new DefaultValuesSettings
                {
                    DefaultLoyaltyScore = 50,
                    DefaultCondition = 100,
                    DefaultMoodStatus = "Grumpy"
                }
            };
        }

        // Public properties for easy access
        public string DatabasePath => _settings.Database.Path;
        public int ConnectionTimeout => _settings.Database.ConnectionTimeout;

        public int MaxMinionsPerScheme => _settings.BusinessRules.MaxMinionsPerScheme;
        public int MaxEquipmentPerScheme => _settings.BusinessRules.MaxEquipmentPerScheme;
        public decimal DefaultMinionSalary => _settings.BusinessRules.DefaultMinionSalary;
        public int LoyaltyDecayRate => _settings.BusinessRules.LoyaltyDecayRate;
        public int LoyaltyGrowthRate => _settings.BusinessRules.LoyaltyGrowthRate;
        public int ConditionDegradationRate => _settings.BusinessRules.ConditionDegradationRate;
        public decimal MaintenanceCostPercentage => _settings.BusinessRules.MaintenanceCostPercentage;
        public decimal DoomsdayMaintenanceCostPercentage => _settings.BusinessRules.DoomsdayMaintenanceCostPercentage;

        public int LowLoyaltyThreshold => _settings.Thresholds.LowLoyaltyThreshold;
        public int HighLoyaltyThreshold => _settings.Thresholds.HighLoyaltyThreshold;
        public int OverworkedDays => _settings.Thresholds.OverworkedDays;
        public int SpecialistSkillLevel => _settings.Thresholds.SpecialistSkillLevel;
        public int MinEquipmentCondition => _settings.Thresholds.MinEquipmentCondition;
        public int BrokenEquipmentCondition => _settings.Thresholds.BrokenEquipmentCondition;
        public int HighDiabolicalRating => _settings.Thresholds.HighDiabolicalRating;
        public int SuccessLikelihoodHighThreshold => _settings.Thresholds.SuccessLikelihoodHighThreshold;
        public int SuccessLikelihoodLowThreshold => _settings.Thresholds.SuccessLikelihoodLowThreshold;

        public RangeSettings SkillLevelRange => _settings.ValidationRanges.SkillLevel;
        public RangeSettings LoyaltyScoreRange => _settings.ValidationRanges.LoyaltyScore;
        public RangeSettings ConditionRange => _settings.ValidationRanges.Condition;
        public RangeSettings DiabolicalRatingRange => _settings.ValidationRanges.DiabolicalRating;
        public RangeSettings SecurityLevelRange => _settings.ValidationRanges.SecurityLevel;
        public RangeSettings SuccessLikelihoodRange => _settings.ValidationRanges.SuccessLikelihood;

        public string[] ValidSpecialties => _settings.ValidValues.MinionSpecialties;
        public string[] ValidMoodStatuses => _settings.ValidValues.MoodStatuses;
        public string[] ValidSchemeStatuses => _settings.ValidValues.SchemeStatuses;
        public string[] ValidCategories => _settings.ValidValues.EquipmentCategories;

        public int DefaultLoyaltyScore => _settings.DefaultValues.DefaultLoyaltyScore;
        public int DefaultCondition => _settings.DefaultValues.DefaultCondition;
        public string DefaultMoodStatus => _settings.DefaultValues.DefaultMoodStatus;

        // Mood status constants for easy access
        public string MoodHappy => "Happy";
        public string MoodGrumpy => "Grumpy";
        public string MoodBetrayal => "Plotting Betrayal";
        public string MoodExhausted => "Exhausted";

        // Scheme status constants for easy access
        public string StatusPlanning => "Planning";
        public string StatusActive => "Active";
        public string StatusOnHold => "On Hold";
        public string StatusCompleted => "Completed";
        public string StatusFailed => "Failed";
    }

    // Configuration classes matching JSON structure
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; }
        public BusinessRulesSettings BusinessRules { get; set; }
        public ThresholdsSettings Thresholds { get; set; }
        public ValidationRangesSettings ValidationRanges { get; set; }
        public ValidValuesSettings ValidValues { get; set; }
        public DefaultValuesSettings DefaultValues { get; set; }
    }

    public class DatabaseSettings
    {
        public string Path { get; set; }
        public int ConnectionTimeout { get; set; }
    }

    public class BusinessRulesSettings
    {
        public int MaxMinionsPerScheme { get; set; }
        public int MaxEquipmentPerScheme { get; set; }
        public decimal DefaultMinionSalary { get; set; }
        public int LoyaltyDecayRate { get; set; }
        public int LoyaltyGrowthRate { get; set; }
        public int ConditionDegradationRate { get; set; }
        public decimal MaintenanceCostPercentage { get; set; }
        public decimal DoomsdayMaintenanceCostPercentage { get; set; }
    }

    public class ThresholdsSettings
    {
        public int LowLoyaltyThreshold { get; set; }
        public int HighLoyaltyThreshold { get; set; }
        public int OverworkedDays { get; set; }
        public int SpecialistSkillLevel { get; set; }
        public int MinEquipmentCondition { get; set; }
        public int BrokenEquipmentCondition { get; set; }
        public int HighDiabolicalRating { get; set; }
        public int SuccessLikelihoodHighThreshold { get; set; }
        public int SuccessLikelihoodLowThreshold { get; set; }
    }

    public class ValidationRangesSettings
    {
        public RangeSettings SkillLevel { get; set; }
        public RangeSettings LoyaltyScore { get; set; }
        public RangeSettings Condition { get; set; }
        public RangeSettings DiabolicalRating { get; set; }
        public RangeSettings SecurityLevel { get; set; }
        public RangeSettings SuccessLikelihood { get; set; }
    }

    public class RangeSettings
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class ValidValuesSettings
    {
        public string[] MinionSpecialties { get; set; }
        public string[] MoodStatuses { get; set; }
        public string[] SchemeStatuses { get; set; }
        public string[] EquipmentCategories { get; set; }
    }

    public class DefaultValuesSettings
    {
        public int DefaultLoyaltyScore { get; set; }
        public int DefaultCondition { get; set; }
        public string DefaultMoodStatus { get; set; }
    }
}
