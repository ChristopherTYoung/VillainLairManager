# Super Villain Lair Management System

## 🦹 Educational Legacy Code Training Application

This is an intentionally messy C# WinForms application designed for Software Maintenance students to practice refactoring, testing, and working with legacy code.

**⚠️ WARNING: This code contains deliberate anti-patterns and testability issues!**

## Purpose

This application demonstrates common problems found in legacy codebases:
- Static singleton database access (no dependency injection)
- Business logic mixed into UI event handlers
- Duplicated code across multiple files
- Hardcoded configuration values
- Tight coupling between layers
- No automated tests

Your mission: Make it testable!

## System Requirements

- **.NET 9.0 SDK** or later
- **Windows** (for WinForms support)
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **SQLite** (included via NuGet package)

## Building the Application

### Option 1: Command Line

```bash
cd VillainLairManager
dotnet restore
dotnet build
dotnet run
```

### Option 2: Visual Studio

1. Open `VillainLairManager.csproj` in Visual Studio 2022
2. Press F5 to build and run

### First Run

On first launch, the application will:
1. Create `villainlair.db` in the application directory
2. Create database schema (4 tables)
3. Seed initial data (10 minions, 5 schemes, 4 bases, 8 equipment items)
4. Open the main dashboard

## Application Features

### Main Dashboard
- View statistics: minion count, scheme success rates, monthly costs
- Alert panel for low loyalty, broken equipment, over-budget schemes
- Navigation to all management screens

### Minion Management (TODO: Complete Implementation)
- CRUD operations for minions
- Assign minions to bases and schemes
- Track loyalty and mood
- Validation for skill levels and specialties

### Evil Scheme Management (TODO: Complete Implementation)
- CRUD operations for schemes
- Track budget and spending
- Calculate success likelihood
- Assign resources (minions and equipment)
- Status transitions (Planning → Active → Completed/Failed)

### Secret Base Management (TODO: Complete Implementation)
- CRUD operations for bases
- Monitor occupancy vs. capacity
- Track maintenance costs
- Evacuate base functionality

### Equipment Inventory (TODO: Complete Implementation)
- CRUD operations for equipment
- Track condition (degrades over time)
- Perform maintenance
- Assign to schemes

## Database Schema

**Tables**:
- `Minions` - Henchmen with skills, loyalty, and assignments
- `EvilSchemes` - Evil plans with budgets and resource requirements
- `SecretBases` - Hidden lairs with capacity and security
- `Equipment` - Tools, weapons, and doomsday devices

See `data-model.md` for detailed schema.

## Anti-Patterns (Intentional!)

### 1. Static Singleton DatabaseHelper
**File**: `DatabaseHelper.cs` (800+ lines)
- All database operations in one static class
- Cannot be mocked or substituted for testing
- Tight coupling throughout application

**Fix**: Introduce `IRepository<T>` interfaces and dependency injection

### 2. Business Logic in UI Event Handlers
**File**: `Forms/MainForm.cs`
- Success likelihood calculation in `LoadStatistics()`
- Loyalty calculations duplicated in UI
- Budget validation in button click handlers

**Fix**: Extract to separate service classes

### 3. Duplicated Code
- Success likelihood calculation in:
  - `EvilScheme.CalculateSuccessLikelihood()` (Models)
  - `MainForm.LoadStatistics()` (UI)
  - SchemeManagementForm (when implemented)
- Specialty validation in:
  - `ValidationHelper.IsValidSpecialty()`
  - `Minion.IsValidSpecialty()`
  - Form ComboBoxes (hardcoded)

**Fix**: Consolidate into single, testable location

### 4. Hardcoded Configuration
**File**: `Utils/ConfigManager.cs`
- Database path hardcoded
- Magic numbers and strings everywhere
- No configuration file

**Fix**: Use `appsettings.json` or similar

### 5. Mixed Concerns in Models
**Files**: `Models/*.cs`
- `Minion.UpdateMood()` calls database directly
- `EvilScheme.CalculateSuccessLikelihood()` queries database
- `Equipment.PerformMaintenance()` saves to database

**Fix**: Separate data models from business logic

## Code Statistics

- **Total Lines**: ~2100 (matches educational target)
- **Files**: 15 total
- **Classes**: 9 (4 models, 5 forms, 2 utilities)
- **Test Coverage**: 0% (no tests exist)

## Implementation Status

### ✅ Complete
- Project structure
- Database schema and helper
- Model classes with business logic
- Utility classes (ConfigManager, ValidationHelper)
- Main dashboard (fully functional)
- Database seeding

### 🚧 Partially Complete (Stubs)
- MinionManagementForm
- SchemeManagementForm
- BaseManagementForm
- EquipmentInventoryForm

## Completing the Implementation

The stub forms need full CRUD implementations. Follow the anti-pattern guidelines:

### MinionManagementForm Example Structure

```csharp
public partial class MinionManagementForm : Form
{
    private DataGridView dgvMinions;
    private TextBox txtName, txtSkillLevel, txtSalary;
    private ComboBox cboSpecialty, cboBase, cboScheme;
    private Button btnAdd, btnUpdate, btnDelete, btnRefresh;

    private void btnAdd_Click(object sender, EventArgs e)
    {
        // Validation logic directly in event handler (anti-pattern)
        if (string.IsNullOrEmpty(txtName.Text))
        {
            MessageBox.Show("Name is required!");
            return;
        }

        // Hardcoded specialty validation (duplicates ValidationHelper)
        string specialty = cboSpecialty.SelectedItem?.ToString();
        if (specialty != "Hacking" && specialty != "Combat" && /* ... */)
        {
            MessageBox.Show("Invalid specialty!");
            return;
        }

        // Direct database call from UI (anti-pattern)
        var minion = new Minion
        {
            Name = txtName.Text,
            SkillLevel = int.Parse(txtSkillLevel.Text),
            Specialty = specialty,
            LoyaltyScore = 50, // Hardcoded default
            SalaryDemand = decimal.Parse(txtSalary.Text),
            MoodStatus = "Grumpy",
            LastMoodUpdate = DateTime.Now
        };

        DatabaseHelper.InsertMinion(minion);
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        // Direct database call
        dgvMinions.DataSource = null;
        dgvMinions.DataSource = DatabaseHelper.GetAllMinions();
    }

    // More button handlers with business logic...
}
```

Follow similar patterns for other forms - keep business logic in event handlers!

## Student Exercises

### Phase 1: Understanding (1-2 hours)
1. Run the application and explore all features
2. Review the codebase structure
3. Identify at least 5 anti-patterns
4. Document where business logic is located

### Phase 2: Refactoring (5-10 hours)
1. Extract hardcoded values to configuration
2. Create repository interfaces for database access
3. Extract business logic into service classes
4. Implement dependency injection
5. Eliminate code duplication

### Phase 3: Testing (5-8 hours)
1. Add xUnit or NUnit test project
2. Write unit tests for business logic
3. Mock database dependencies
4. Achieve 50%+ code coverage
5. Write integration tests for database operations

## Business Rules to Test

See `contracts/` for detailed specifications:

**Minion Rules**:
- Loyalty decay/growth based on salary
- Mood determination from loyalty
- Assignment validation (skill/specialty matching)
- Base capacity enforcement

**Scheme Rules**:
- Success likelihood calculation (complex!)
- Budget enforcement
- Status transitions
- Resource requirements

**Equipment Rules**:
- Condition degradation over time
- Maintenance cost calculation
- Assignment validation
- Specialist requirements

## Example Test Cases

```csharp
[Fact]
public void CalculateSuccessLikelihood_WithMatchingMinions_ReturnsHigherSuccess()
{
    // Arrange
    var scheme = new EvilScheme
    {
        RequiredSpecialty = "Hacking",
        RequiredSkillLevel = 6
    };
    var minions = new List<Minion>
    {
        new Minion { Specialty = "Hacking", SkillLevel = 8 },
        new Minion { Specialty = "Hacking", SkillLevel = 7 }
    };

    // Act
    int success = CalculateSuccess(scheme, minions, emptyEquipment);

    // Assert
    Assert.InRange(success, 60, 80); // Expected range with 2 matching minions
}
```

## Common Refactoring Patterns

### Pattern 1: Repository Pattern
```csharp
public interface IMinionRepository
{
    List<Minion> GetAll();
    Minion GetById(int id);
    void Insert(Minion minion);
    void Update(Minion minion);
    void Delete(int id);
}

public class SqliteMinionRepository : IMinionRepository
{
    private readonly IDbConnection _connection;

    public SqliteMinionRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    // Implementation using _connection instead of static DatabaseHelper
}
```

### Pattern 2: Service Layer
```csharp
public interface ISchemeService
{
    int CalculateSuccessLikelihood(EvilScheme scheme);
    bool CanActivateScheme(EvilScheme scheme);
    void AssignMinion(int schemeId, int minionId);
}

public class SchemeService : ISchemeService
{
    private readonly ISchemeRepository _schemeRepo;
    private readonly IMinionRepository _minionRepo;

    public SchemeService(ISchemeRepository schemeRepo, IMinionRepository minionRepo)
    {
        _schemeRepo = schemeRepo;
        _minionRepo = minionRepo;
    }

    public int CalculateSuccessLikelihood(EvilScheme scheme)
    {
        // Consolidated business logic here
        // Can be tested without database or UI
    }
}
```

### Pattern 3: Dependency Injection
```csharp
// In Program.cs
var services = new ServiceCollection();
services.AddSingleton<IDbConnection>(new SQLiteConnection("..."));
services.AddScoped<IMinionRepository, SqliteMinionRepository>();
services.AddScoped<ISchemeService, SchemeService>();
var provider = services.BuildServiceProvider();

// In forms
public MinionManagementForm(IMinionRepository minionRepo, ISchemeService schemeService)
{
    _minionRepo = minionRepo;
    _schemeService = schemeService;
    // Now testable!
}
```

## Resources

- [Technical Plan](../specs/001-untangle-training-app/plan.md)
- [Data Model](../specs/001-untangle-training-app/data-model.md)
- [Business Rules](../specs/001-untangle-training-app/contracts/)
- [Student Quickstart](../specs/001-untangle-training-app/quickstart.md)

## Troubleshooting

**Database file not found**:
- Check that `villainlair.db` was created in application directory
- Delete and restart to reseed

**Build errors**:
- Ensure .NET 9.0 SDK is installed
- Run `dotnet restore`

**Forms not displaying**:
- Ensure running on Windows
- Check that `<UseWindowsForms>true</UseWindowsForms>` is in .csproj

## License

This is educational software for Software Maintenance courses. Use and modify freely for educational purposes.

---

**Remember**: The goal isn't to rewrite everything - it's to make strategic refactorings that improve testability while preserving functionality. Real-world legacy code maintenance is incremental improvement, not big-bang rewrites!

Good luck, and may your tests be green! 🧪✅
