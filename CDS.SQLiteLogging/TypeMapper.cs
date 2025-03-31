//using System.Collections.Immutable;
//using System.Reflection;

//namespace CDS.SQLiteLogging;

///// <summary>
///// Provides mapping functionality between .NET types and SQLite column types.
///// </summary>
//public static class TypeMapper
//{
//    /// <summary>
//    /// Creates an immutable dictionary that maps .NET types to SQLite column types.
//    /// </summary>
//    /// <param name="properties">The properties to consider for mapping enum types.</param>
//    /// <returns>An immutable dictionary mapping .NET types to SQLite types.</returns>
//    public static ImmutableDictionary<Type, string> CreateTypeToSqliteMap(IEnumerable<PropertyInfo> properties)
//    {
//        var typeMap = new Dictionary<Type, string>
//        {
//            { typeof(int), "INTEGER" },
//            { typeof(Enum), "INTEGER" },
//            { typeof(long), "INTEGER" },
//            { typeof(string), "TEXT" },
//            { typeof(DateTime), "TEXT" },
//            { typeof(DateTimeOffset), "TEXT" },
//            { typeof(bool), "INTEGER" },
//            { typeof(double), "REAL" },
//            { typeof(float), "REAL" },
//            { typeof(decimal), "REAL" },
//        };

//        // Add specific enum types to the mapping
//        foreach (var prop in properties)
//        {
//            if (prop.PropertyType.IsEnum)
//            {
//                typeMap[prop.PropertyType] = "INTEGER";
//            }
//        }

//        return typeMap.ToImmutableDictionary();
//    }

//    /// <summary>
//    /// Maps a .NET type to an equivalent SQLite column type.
//    /// </summary>
//    /// <param name="type">The .NET type to map.</param>
//    /// <param name="typeMap">The type mapping dictionary.</param>
//    /// <returns>The SQLite column type as a string.</returns>
//    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
//    public static string MapTypeToSqliteType(Type type, ImmutableDictionary<Type, string> typeMap)
//    {
//        if (type.IsEnum)
//        {
//            return "INTEGER";
//        }

//        if (typeMap.TryGetValue(type, out string sqliteType))
//        {
//            return sqliteType;
//        }
        
//        if (type == typeof(IReadOnlyDictionary<string, object>))
//        {
//            return "TEXT";
//        }

//        throw new NotSupportedException($"Property type {type.Name} is not supported.");
//    }
//}
