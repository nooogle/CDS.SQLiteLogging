//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace CDS.SQLiteLogging.Microsoft;


//public class MSSQLiteLoggerFactory : ILoggerFactory
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly SQLiteLoggerProvider _sqliteLoggerProvider;
//    private readonly List<ILoggerProvider> _providers = new List<ILoggerProvider>();

//    public MSSQLiteLoggerFactory(IServiceProvider serviceProvider, SQLiteLoggerProvider sqliteLoggerProvider)
//    {
//        _serviceProvider = serviceProvider;
//        _sqliteLoggerProvider = sqliteLoggerProvider;
//        _providers.Add(sqliteLoggerProvider);
//    }

//    public IMSSQLiteLogger CreateLogger(string categoryName)
//    {
//        var scopeProvider = _serviceProvider.GetRequiredService<IExternalScopeProvider>();
//        return new MSSQLiteLogger(categoryName, _sqliteLoggerProvider.CreateLogger(categoryName) as SQLiteLogger, scopeProvider);
//    }

//    ILogger ILoggerFactory.CreateLogger(string categoryName)
//    {
//        return CreateLogger(categoryName);
//    }

//    public void AddProvider(ILoggerProvider provider)
//    {
//        _providers.Add(provider);
//    }

//    public void Dispose()
//    {
//        foreach (var provider in _providers)
//        {
//            provider.Dispose();
//        }
//    }
//}

