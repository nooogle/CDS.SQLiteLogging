using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CDS.SQLiteLogging;

/// <summary>
/// Creates tables in SQLite databases based on .NET type definitions.
/// </summary>
public class TableCreator
{
    private readonly ConnectionManager connectionManager;
    private readonly ImmutableDictionary<Type, string> typeToSqliteMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCreator"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="typeToSqliteMap">The type mapping dictionary.</param>
    public TableCreator(ConnectionManager connectionManager, ImmutableDictionary<Type, string> typeToSqliteMap)
    {
        this.connectionManager = connectionManager;
        this.typeToSqliteMap = typeToSqliteMap;
    }

    /// <summary>
    /// Creates a table based on the public non-static properties of the specified type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type used to define the table schema.</typeparam>
    /// <returns>The name of the created table.</returns>
    public string CreateTableForType<TEntity>()
    {
        string tableName = typeof(TEntity).Name;
        PropertyInfo[] properties = GetPublicNonStaticProperties<TEntity>();
        var columnDefinitions = new List<string>();

        foreach (var prop in properties)
        {
            string columnDef = GetColumnDefinition(prop);
            if (!string.IsNullOrEmpty(columnDef))
            {
                columnDefinitions.Add(columnDef);
            }
        }

        string sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columnDefinitions)});";
        connectionManager.ExecuteNonQuery(sql);

        return tableName;
    }


    /// <summary>
    /// Returns the public non-static properties of the specified type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type used to define the table schema.
    /// </typeparam>
    /// <returns>
    /// A list of public non-static properties.
    /// </returns>
    public static PropertyInfo[] GetPublicNonStaticProperties<TEntity>()
    {
        return typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => !p.GetGetMethod().IsStatic)
                                        .ToArray();
    }

    /// <summary>
    /// Returns the SQL column definition for the specified property.
    /// </summary>
    /// <param name="prop">The property to map.</param>
    /// <returns>The SQL column definition string.</returns>
    private string GetColumnDefinition(PropertyInfo prop)
    {
        if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && prop.PropertyType == typeof(int))
        {
            return "Id INTEGER PRIMARY KEY AUTOINCREMENT";
        }

        string sqliteType = TypeMapper.MapTypeToSqliteType(prop.PropertyType, typeToSqliteMap);
        if (string.IsNullOrEmpty(sqliteType))
        {
            throw new NotSupportedException($"Property type {prop.PropertyType.Name} is not supported.");
        }
        
        return $"{prop.Name} {sqliteType}";
    }
}
