using System;
using System.Data.SQLite;
using System.Windows.Forms;
using VillainLairManager.Forms;
using VillainLairManager.Utils;
using VillainLairManager.Repositories;
using VillainLairManager.Services;

namespace VillainLairManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application with service layer dependency injection.
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

            // Create repositories (data access layer)
            var minionRepository = new MinionRepository(connection);
            var equipmentRepository = new EquipmentRepository(connection);
            var schemeRepository = new EvilSchemeRepository(connection);
            var baseRepository = new SecretBaseRepository(connection);

            // Create services (business logic layer)
            var minionService = new MinionService(minionRepository);
            var equipmentService = new EquipmentService(equipmentRepository, schemeRepository);
            var schemeService = new SchemeService(schemeRepository, minionRepository, equipmentRepository);
            var baseService = new BaseService(baseRepository, minionRepository);
            var statisticsService = new StatisticsService(minionService, schemeService, baseService, equipmentService);

            // Create form factory functions for proper DI with services
            Func<MinionManagementForm> createMinionForm = () => 
                new MinionManagementForm(minionService, baseRepository, schemeRepository);
            
            Func<EquipmentInventoryForm> createEquipmentForm = () => 
                new EquipmentInventoryForm(equipmentService);
            
            Func<SchemeManagementForm> createSchemeForm = () => 
                new SchemeManagementForm(schemeRepository);
            
            Func<BaseManagementForm> createBaseForm = () => 
                new BaseManagementForm(baseRepository);

            // Create and run main form with service dependencies
            var mainForm = new MainForm(
                statisticsService,
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
