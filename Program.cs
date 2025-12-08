using System;
using System.Data.SQLite;
using System.Windows.Forms;
using VillainLairManager.Forms;
using VillainLairManager.Utils;
using VillainLairManager.Repositories;

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

            // Initialize database using DatabaseHelper for schema creation and seeding
            DatabaseHelper.Initialize();
            DatabaseHelper.CreateSchemaIfNotExists();
            DatabaseHelper.SeedInitialData();

            // Get the connection from DatabaseHelper for repositories
            var connection = DatabaseHelper.GetConnection();

            // Create repositories (dependency injection setup)
            var minionRepository = new MinionRepository(connection);
            var equipmentRepository = new EquipmentRepository(connection);
            var schemeRepository = new EvilSchemeRepository(connection);
            var baseRepository = new SecretBaseRepository(connection);

            // Create form factory functions for proper DI
            Func<MinionManagementForm> createMinionForm = () => 
                new MinionManagementForm(minionRepository, baseRepository, schemeRepository);
            
            Func<EquipmentInventoryForm> createEquipmentForm = () => 
                new EquipmentInventoryForm(equipmentRepository);
            
            Func<SchemeManagementForm> createSchemeForm = () => 
                new SchemeManagementForm(schemeRepository);
            
            Func<BaseManagementForm> createBaseForm = () => 
                new BaseManagementForm(baseRepository);

            // Create and run main form with all dependencies
            var mainForm = new MainForm(
                minionRepository,
                schemeRepository,
                baseRepository,
                equipmentRepository,
                createMinionForm,
                createEquipmentForm,
                createSchemeForm,
                createBaseForm
            );

            Application.Run(mainForm);

            // Clean up connection on exit
            connection.Close();
            connection.Dispose();
        }
    }
}
