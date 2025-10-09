# Database Schema Centralization - Implementation Summary

## Overview
This document describes the centralization of SQLite database schema definitions in the CDS.SQLiteLogging project to ensure single source of truth for all table and column names.

## Problem Statement
Previously, table and column names were hardcoded as strings throughout the codebase, leading to:
- **Duplication**: Column names repeated in multiple files
- **High risk of errors**: Changing a column name required finding and updating all occurrences
- **Maintenance difficulty**: No guarantee that all instances were updated consistently
- **No compile-time safety**: Typos in column names would only be caught at runtime

## Solution
Created a centralized `DatabaseSchema` class in `CDS.SQLiteLogging\Internal\DatabaseSchema.cs` that provides:
- Const string definitions for all table and column names
- Helper methods for common operations (getting all columns, insertable columns, column definitions)
- Single source of truth for schema changes
- Compile-time checking of column name references

## Implementation Details

### New File Created
**File**: `CDS.SQLiteLogging\Internal\DatabaseSchema.cs`

**Structure**:
```csharp
internal static class DatabaseSchema
{
    public static class Tables
    {
        public const string LogEntry = nameof(LogEntry);
    }

    public static class Columns
    {
        public const string DbId = nameof(DbId);
        public const string Category = nameof(Category);
        public const string EventId = nameof(EventId);
        public const string EventName = nameof(EventName);
        public const string Timestamp = nameof(Timestamp);
        public const string Level = nameof(Level);
        public const string ManagedThreadId = nameof(ManagedThreadId);
        public const string MessageTemplate = nameof(MessageTemplate);
        public const string Properties = nameof(Properties);
        public const string RenderedMessage = nameof(RenderedMessage);
        public const string ExceptionJson = nameof(ExceptionJson);
        public const string ScopesJson = nameof(ScopesJson);
    }

    public static string[] GetAllColumns() { ... }
    public static string[] GetInsertableColumns() { ... }
    public static List<string> GetColumnDefinitions() { ... }
}
```

### Files Modified

#### 1. **TableCreator.cs**
- **Changed**: Uses `DatabaseSchema.Tables.LogEntry` for table name
- **Changed**: Uses `DatabaseSchema.GetColumnDefinitions()` for CREATE TABLE statement
- **Benefit**: Column definitions centralized; adding/modifying columns only requires changes in DatabaseSchema

#### 2. **LogWriter.cs**
- **Changed**: Uses `DatabaseSchema.GetInsertableColumns()` to build INSERT statement
- **Changed**: Uses `DatabaseSchema.Columns.*` for parameter names
- **Benefit**: INSERT statements automatically include all insertable columns; adding new columns requires no changes here

#### 3. **Reader.cs**
- **Changed**: Uses `DatabaseSchema.Tables.LogEntry` for table name
- **Changed**: Uses `DatabaseSchema.Columns.*` in SELECT statements and GetOrdinal() calls
- **Benefit**: All column references checked at compile time; changing column names is safe

#### 4. **Housekeeper.cs**
- **Changed**: Uses `DatabaseSchema.Tables.LogEntry` and `DatabaseSchema.Columns.*` in DELETE and WHERE clauses
- **Benefit**: Housekeeping operations remain in sync with schema changes

#### 5. **DirectDBExporter.cs**
- **Changed**: Uses `DatabaseSchema.GetInsertableColumns()` for building INSERT statements
- **Changed**: Uses `DatabaseSchema.Columns.*` in ColumnOrdinals class and parameter binding
- **Changed**: Uses `DatabaseSchema.Columns.DbId` in WHERE and ORDER BY clauses
- **Benefit**: Export functionality automatically handles schema changes

## Benefits Achieved

### 1. **Single Source of Truth**
- All schema information is in one place
- Changes to column names only need to be made once
- Reduces risk of inconsistencies

### 2. **Compile-Time Safety**
- Typos in column names cause compilation errors instead of runtime failures
- IDE IntelliSense helps developers find correct column names
- Refactoring tools can track all usages

### 3. **Easier Maintenance**
- Adding a new column:
  1. Add const to `DatabaseSchema.Columns`
  2. Add to `GetColumnDefinitions()`
  3. Add to `GetInsertableColumns()` (if not auto-increment)
  4. Update `LogEntry` class properties
  5. All SQL queries automatically include the new column

### 4. **Documentation**
- Schema is self-documenting with XML comments
- Helper methods explain the purpose of each column set
- Developers can see the entire schema at a glance

### 5. **Testing**
- Unit tests can verify schema consistency
- Mock schema can be created for testing without database access

## Example: Adding a New Column

**Before** (required changes in multiple files):
```csharp
// TableCreator.cs
"NewColumn TEXT"

// LogWriter.cs
"INSERT INTO LogEntry (..., NewColumn) VALUES (..., @NewColumn)"
cmd.Parameters.AddWithValue("@NewColumn", entry.NewColumn);

// Reader.cs
NewColumn = reader.GetString(reader.GetOrdinal("NewColumn"))

// Housekeeper.cs
// May need updates depending on usage

// DirectDBExporter.cs
"INSERT INTO ... (NewColumn) VALUES (@NewColumn)"
insertCmd.Parameters.AddWithValue("@NewColumn", ...);
```

**After** (centralized approach):
```csharp
// 1. DatabaseSchema.cs
public static class Columns
{
    // ...existing columns...
    public const string NewColumn = nameof(NewColumn);
}

// 2. Update GetColumnDefinitions()
$"{Columns.NewColumn} TEXT"

// 3. Update GetInsertableColumns()
Columns.NewColumn

// 4. LogEntry.cs
public string? NewColumn { get; set; }

// 5. Reader.cs CreateLogEntryFromReader()
NewColumn = reader.GetString(reader.GetOrdinal(Internal.DatabaseSchema.Columns.NewColumn))
```

**That's it!** All SQL generation code automatically includes the new column.

## Migration Notes

### Backward Compatibility
- ? **Fully backward compatible**: Database schema unchanged
- ? **No database migrations required**: Only code refactoring
- ? **Existing databases work unchanged**: Same table and column names

### Testing Recommendations
1. Run all existing unit tests to ensure no regressions
2. Test with existing database files
3. Test creating new databases
4. Test export/import functionality
5. Verify housekeeping operations

## Future Improvements

### Potential Enhancements
1. **Schema Versioning**: Add schema version tracking for future migrations
2. **Index Definitions**: Centralize index creation statements
3. **Validation**: Add methods to validate schema consistency
4. **Migration Support**: Add helper methods for schema upgrades
5. **Type Safety**: Consider strongly-typed column access patterns

### Example Future Enhancement
```csharp
public static class DatabaseSchema
{
    public const int SchemaVersion = 1;
    
    public static class Indexes
    {
        public const string TimestampIndex = "IX_LogEntry_Timestamp";
        public const string LevelIndex = "IX_LogEntry_Level";
    }
    
    public static List<string> GetIndexDefinitions()
    {
        return
        [
            $"CREATE INDEX IF NOT EXISTS {Indexes.TimestampIndex} ON {Tables.LogEntry}({Columns.Timestamp})",
            $"CREATE INDEX IF NOT EXISTS {Indexes.LevelIndex} ON {Tables.LogEntry}({Columns.Level})"
        ];
    }
}
```

## Conclusion

The centralization of database schema definitions significantly improves code maintainability, reduces the risk of errors, and provides compile-time safety for all database operations. This change follows software engineering best practices and aligns with the DRY (Don't Repeat Yourself) principle.

All changes are backward compatible and require no database migrations, making this a low-risk, high-value improvement to the codebase.
