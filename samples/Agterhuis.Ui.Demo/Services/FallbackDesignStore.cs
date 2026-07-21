using Agterhuis.Ui.Designer.Model;
using Agterhuis.Ui.Designer.Persistence;

namespace Agterhuis.Ui.Demo.Services;

public sealed class FallbackDesignStore(LocalDesignStore localStore, RemoteDesignStore remoteStore, DesignerPersistenceMode mode = DesignerPersistenceMode.Auto) : IDesignStore
{
    private bool? _remoteAvailable;

    public bool IsOfflineFallbackActive { get; private set; }

    public async Task<IReadOnlyList<DesignListItem>> GetRecentAsync()
    {
        return await ExecuteWithFallbackAsync(
            static store => store.GetRecentAsync(),
            static store => store.GetRecentAsync());
    }

    public async Task<DesignDocumentEnvelope?> LoadAsync(string name, int? version = null)
    {
        return await ExecuteWithFallbackAsync(
            store => store.LoadAsync(name, version),
            store => store.LoadAsync(name, version));
    }

    public async Task<DesignDocumentEnvelope> SaveAsync(string name, DesignDocument document, string? expectedETag)
    {
        return await ExecuteWithFallbackAsync(
            store => store.SaveAsync(name, document, expectedETag),
            store => store.SaveAsync(name, document, expectedETag));
    }

    public async Task RemoveAsync(string name)
    {
        await ExecuteWithFallbackAsync(
            async store =>
            {
                await store.RemoveAsync(name);
                return true;
            },
            async store =>
            {
                await store.RemoveAsync(name);
                return true;
            });
    }

    public async Task<IReadOnlyList<DesignVersionInfo>> GetVersionsAsync(string name)
    {
        return await ExecuteWithFallbackAsync(
            store => store.GetVersionsAsync(name),
            store => store.GetVersionsAsync(name));
    }

    public async Task<DesignDocumentEnvelope?> RestoreVersionAsync(string name, int version)
    {
        return await ExecuteWithFallbackAsync(
            store => store.RestoreVersionAsync(name, version),
            store => store.RestoreVersionAsync(name, version));
    }

    public async Task<bool> EnsureRemoteAvailableAsync()
    {
        if (mode == DesignerPersistenceMode.Local)
        {
            _remoteAvailable = false;
            IsOfflineFallbackActive = true;
            return false;
        }

        if (mode == DesignerPersistenceMode.Remote)
        {
            _remoteAvailable = true;
            IsOfflineFallbackActive = false;
            return true;
        }

        if (_remoteAvailable.HasValue)
        {
            return _remoteAvailable.Value;
        }

        try
        {
            _ = await remoteStore.GetRecentAsync();
            _remoteAvailable = true;
            IsOfflineFallbackActive = false;
            return true;
        }
        catch
        {
            _remoteAvailable = false;
            IsOfflineFallbackActive = true;
            return false;
        }
    }

    private async Task<T> ExecuteWithFallbackAsync<T>(Func<RemoteDesignStore, Task<T>> remoteAction, Func<LocalDesignStore, Task<T>> localAction)
    {
        var useRemote = mode switch
        {
            DesignerPersistenceMode.Remote => true,
            DesignerPersistenceMode.Local => false,
            _ => await EnsureRemoteAvailableAsync()
        };

        if (useRemote)
        {
            try
            {
                var result = await remoteAction(remoteStore);
                IsOfflineFallbackActive = false;
                return result;
            }
            catch (DesignConflictException)
            {
                throw;
            }
            catch
            {
                if (mode == DesignerPersistenceMode.Remote)
                {
                    throw;
                }

                _remoteAvailable = false;
            }
        }

        IsOfflineFallbackActive = true;
        return await localAction(localStore);
    }
}
