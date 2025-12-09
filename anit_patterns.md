## Anti-Patterns Found

### 1. God Class (DatabaseHelper)

**Where:** [DatabaseHelper.cs](DatabaseHelper.cs) - entire file

**The Problem:**
One massive static class handles ALL database stuff - Minions, Schemes, Bases, Equipment. Everything goes through this one class.

**Why it's problematic and how it impacts testability and maintainability:**
- Can't write unit tests without hitting the actual database
- Can't swap in fake data for testing
- If you change something, it might break everything
- Can't switch to a different database later

**Code Example:**
```csharp
public static class DatabaseHelper
{
    private static SQLiteConnection _connection = null;

    public static void Initialize() { ... }
    public static List<Minion> GetAllMinions() { ... }
    public static void DeleteMinion(int id) { ... }
    }
```

---

### 2. Business Logic in Forms

**Where:**
- [Forms/MainForm.cs:48-138](Forms/MainForm.cs#L48-L138)
- [Forms/MinionManagementForm.cs:28-246](Forms/MinionManagementForm.cs#L28-L246)

**The Problem:**
The UI code is doing calculations and making business decisions. Forms shouldn't know about loyalty scores or success calculations.

**Why it's problematic and how it impacts testability and maintainability:**
- Can't test business logic without opening the actual UI
- Can't reuse the logic anywhere else (like in an API)
- Makes the forms really complicated

**Code Example:**
```csharp
foreach (var minion in minions)
{
    if (minion.LoyaltyScore > 70)
        happyCount++;
    else if (minion.LoyaltyScore < 40)
        betrayalCount++;
}
```

---

### 3. Copy-Paste Everywhere

**Where:** The same code appears in like 5 different files:
- [Forms/MinionManagementForm.cs:37-46](Forms/MinionManagementForm.cs#L37-L46)
- [Utils/ValidationHelper.cs:12-18](Utils/ValidationHelper.cs#L12-L18)
- [Models/Minion.cs:40-46](Models/Minion.cs#L40-L46)
- [Utils/ConfigManager.cs:37-45](Utils/ConfigManager.cs#L37-45)

**The Problem:**
Validation rules and mood calculations are copied all over the place. If you fix a bug in one spot, you have to remember to fix it in 4 other spots too.

**Why it's problematic and how it impacts testability and maintainability:**
- Bug fixes have to be applied in multiple places, easy to miss one
- Inconsistencies can appear when one copy gets updated but others don't
- Increases codebase size unnecessarily
- Makes refactoring dangerous because you might miss a duplicate
- Hard to test comprehensively since the same logic exists in different contexts

**Code Example:**
```csharp
// MinionManagementForm.cs
if (specialty != "Hacking" && specialty != "Explosives" &&
    specialty != "Disguise" && specialty != "Combat" &&
    specialty != "Engineering" && specialty != "Piloting")

// ValidationHelper.cs
return specialty == "Hacking" || specialty == "Explosives" ||
       specialty == "Disguise" || specialty == "Combat" ||
       specialty == "Engineering" || specialty == "Piloting";
```

---

### 4. Models Talk to Database Directly

**Where:** All model files
- [Models/Minion.cs:23-66](Models/Minion.cs#L23-L66)
- [Models/EvilScheme.cs:26-77](Models/EvilScheme.cs#L26-L77)
- [Models/Equipment.cs:23-61](Models/Equipment.cs#L23-L61)

**The Problem:**
Models call `DatabaseHelper` directly. So if you want to test a model, you need a real database.

**Why it's problematic and how it impacts testability and maintainability:**
- Can't test models without database setup
- Models are doing too many things (data + logic + database)
- Hard to change how data is saved later
- Violates Single Responsibility Principle - models should represent data, not handle persistence
- Can't mock database interactions for isolated unit tests
- Changes to database schema or technology require modifying model classes

**Code Example:**
```csharp
public void UpdateMood()
{
    this.MoodStatus = "Happy";

    DatabaseHelper.UpdateMinion(this);
}
```

---

### 5. Magic Numbers

**Where:** Throughout the code
- [Forms/MainForm.cs:63-68](Forms/MainForm.cs#L63-L68)
- [Forms/MinionManagementForm.cs:55-58](Forms/MinionManagementForm.cs#L55-L58)

**The Problem:**
Random numbers like 70, 40, 5 appear in the code with no explanation.

**Why it's problematic and how it impacts testability and maintainability:**
- Unclear what these numbers represent or why they were chosen
- Hard to change consistently across the codebase
- Makes code difficult to understand without context
- Can't easily test different threshold values
- If business rules change, finding all magic numbers is error-prone
- No single source of truth for business rule constants

**Code Example:**
```csharp
if (minion.LoyaltyScore > 70)
    happyCount++;
else if (minion.LoyaltyScore < 40)
    betrayalCount++;

int degradation = monthsSinceMaintenance * 5;
```

---

## Business Rules

### Minion Loyalty

**Loyalty Range:** 0-100

**Moods:**
- Loyalty > 70 = Happy
- Loyalty 40-70 = Grumpy
- Loyalty < 40 = Plotting Betrayal

**How Loyalty Changes:**
- Paid enough: +3 loyalty
- Underpaid: -5 loyalty

---

### Scheme Success Calculation

**Base:** Starts at 50%

**Bonuses:**
- +10% for each minion with the right specialty
- +5% for each working equipment (condition >= 50)

**Penalties:**
- -20% if over budget
- -15% if not enough resources (< 2 minions or wrong specialty)
- -25% if past the deadline

**Formula:**
```
success = 50 + (matchingMinions × 10) + (workingEquipment × 5)
          + penalties
Final result clamped to 0-100
```

---

### Equipment Degradation

**Condition Range:** 0-100

**Status:**
- Operational: >= 50
- Broken: < 20

**Degradation:**
- Loses 5 points per month when in use (only for active schemes)

**Maintenance:**
- Regular equipment: costs 15% of purchase price
- Doomsday Device: costs 30% of purchase price
- Restores condition to 100%

---

### Minion Specialties

Valid options:
- Hacking
- Explosives
- Disguise
- Combat
- Engineering
- Piloting

**Skill Level:** 1-10 (8+ is a "specialist")

---

### Other Rules

**Scheme Limits:** (NOT actually enforced in code)
- Max 10 minions per scheme
- Max 5 equipment per scheme

**Monthly Costs:**
```
Total = All minion salaries + All base maintenance + All equipment maintenance
```

**Alerts:**
- Low loyalty warning when < 40
- Broken equipment warning when < 20
- Over budget warning when spending > budget

---

## What Should Be Fixed

1. **Add a Repository layer** - Stop calling DatabaseHelper everywhere
2. **Create Service classes** - Move business logic out of forms
3. **Stop duplicating code** - Put validation in ONE place
4. **Better error handling** - Right now errors just show ugly messages to users
5. **Make it testable** - Use interfaces and dependency injection

## Bottom Line

The app works but is a nightmare to test or change. Everything is tangled together. You'd need to refactor before adding any serious new features.
