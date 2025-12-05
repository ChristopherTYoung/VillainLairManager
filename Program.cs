using System;
using System.Windows.Forms;
using VillainLairManager.Forms;
using VillainLairManager.Utils;

namespace VillainLairManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize configuration first
            var config = ConfigManager.Instance;

            // Initialize database - no error handling (anti-pattern)
            DatabaseHelper.Initialize();

            // Create schema if needed
            DatabaseHelper.CreateSchemaIfNotExists();

            // Seed data on first run - no check if already seeded (anti-pattern)
            DatabaseHelper.SeedInitialData();

            Application.Run(new MainForm());
        }
    }
}
