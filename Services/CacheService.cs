using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public interface ICacheService
    {
        Task<List<Curso>?> GetCursosActivosAsync();
        Task SetCursosActivosAsync(List<Curso> cursos);
        Task RemoveCursosActivosAsync();
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private const string CURSOS_ACTIVOS_KEY = "cursos_activos";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(1); // 60 segundos como pide el examen

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Curso>?> GetCursosActivosAsync()
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(CURSOS_ACTIVOS_KEY);
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cache miss para cursos activos");
                    return null;
                }

                _logger.LogInformation("Cache hit para cursos activos");
                return JsonSerializer.Deserialize<List<Curso>>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cursos activos del cache");
                return null;
            }
        }

        public async Task SetCursosActivosAsync(List<Curso> cursos)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                };

                var serializedData = JsonSerializer.Serialize(cursos);
                await _cache.SetStringAsync(CURSOS_ACTIVOS_KEY, serializedData, options);
                _logger.LogInformation("Cursos activos almacenados en cache por {Duration} minutos", _cacheDuration.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al almacenar cursos activos en cache");
            }
        }

        public async Task RemoveCursosActivosAsync()
        {
            try
            {
                await _cache.RemoveAsync(CURSOS_ACTIVOS_KEY);
                _logger.LogInformation("Cache de cursos activos invalidado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar cache de cursos activos");
            }
        }
    }
}