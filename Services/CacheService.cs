using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public interface ICacheService
    {
        Task<List<CursoCacheDto>?> GetCursosActivosAsync();
        Task SetCursosActivosAsync(List<CursoCacheDto> cursos);
        Task RemoveCursosActivosAsync();
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private const string CURSOS_ACTIVOS_KEY = "cursos_activos";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(1);

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<CursoCacheDto>?> GetCursosActivosAsync()
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(CURSOS_ACTIVOS_KEY);
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("❌ CACHE MISS - No se encontraron cursos en cache");
                    return null;
                }

                _logger.LogInformation("✅ CACHE HIT - Cursos obtenidos desde cache");
                return JsonSerializer.Deserialize<List<CursoCacheDto>>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR al obtener cursos activos del cache");
                return null;
            }
        }

        public async Task SetCursosActivosAsync(List<CursoCacheDto> cursos)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                };

                var serializedData = JsonSerializer.Serialize(cursos);
                await _cache.SetStringAsync(CURSOS_ACTIVOS_KEY, serializedData, options);
                _logger.LogInformation("💾 CACHE SET - {Count} cursos almacenados en cache por {Duration} segundos", 
                    cursos.Count, _cacheDuration.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR al almacenar cursos activos en cache");
            }
        }

        public async Task RemoveCursosActivosAsync()
        {
            try
            {
                await _cache.RemoveAsync(CURSOS_ACTIVOS_KEY);
                _logger.LogInformation("🗑️ CACHE INVALIDADO - Cache de cursos activos eliminado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR al invalidar cache de cursos activos");
            }
        }
    }
}