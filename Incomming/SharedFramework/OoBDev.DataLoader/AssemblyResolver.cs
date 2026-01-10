using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OoBDev.DataLoader
{
    public class AssemblyResolver : IDisposable
    {
        private bool _disposedValue;

        private readonly string _searchPath;
        private readonly ILogger? _logger;

        public AssemblyResolver(string searchPath, ILogger? logger = null)
        {
            _logger = logger;
            if (!string.IsNullOrWhiteSpace(searchPath))
            {
                if (Path.GetFullPath(searchPath) != searchPath)
                    throw new ApplicationException($"{nameof(searchPath)}: \"{searchPath}\" must be a fully qualified path");
                _searchPath = searchPath;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            var fileName = args.Name?.Split(',')?[0]?.Trim();
            var targets = from ext in new[] { ".exe", ".dll" }
                          select Path.Combine(_searchPath, $"{fileName}{ext}");
            foreach (var target in targets)
                if (File.Exists(target))
                {
                    try
                    {
                        return Assembly.LoadFrom(target);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError("Unable to load assembly \"{assembly}\" from \"{path}\" because \"{error}\"", args.Name, target, ex.Message);
                    }
                }

            _logger?.LogWarning("Unable to load assembly \"{assembly}\" from \"{path}\"", args.Name, _searchPath);

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                if (!string.IsNullOrWhiteSpace(_searchPath))
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
                _disposedValue = true;
            }
        }

        ~AssemblyResolver()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
