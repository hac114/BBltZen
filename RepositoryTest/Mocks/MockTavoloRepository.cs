using DTO;
using Repository.Interface;
using System.Collections.Concurrent;

namespace RepositoryTest.Mocks
{
    public class MockTavoloRepository : ITavoloRepository
    {
        private readonly ConcurrentDictionary<int, TavoloDTO> _tavoli = new();
        private int _nextId = 1;

        public Task AddAsync(TavoloDTO tavoloDto)
        {
            tavoloDto.TavoloId = _nextId++;
            _tavoli[tavoloDto.TavoloId] = tavoloDto;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int tavoloId)
        {
            _tavoli.TryRemove(tavoloId, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(int tavoloId)
        {
            return Task.FromResult(_tavoli.ContainsKey(tavoloId));
        }

        public Task<IEnumerable<TavoloDTO>> GetAllAsync()
        {
            return Task.FromResult(_tavoli.Values.AsEnumerable());
        }

        public Task<TavoloDTO> GetByIdAsync(int tavoloId)
        {
            _tavoli.TryGetValue(tavoloId, out var tavolo);
            return Task.FromResult(tavolo);
        }

        public Task<TavoloDTO> GetByNumeroAsync(int numero)
        {
            var tavolo = _tavoli.Values.FirstOrDefault(t => t.Numero == numero);
            return Task.FromResult(tavolo);
        }

        public Task<TavoloDTO> GetByQrCodeAsync(string qrCode)
        {
            var tavolo = _tavoli.Values.FirstOrDefault(t => t.QrCode == qrCode);
            return Task.FromResult(tavolo);
        }

        public Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona)
        {
            var tavoli = _tavoli.Values.Where(t => t.Zona == zona);
            return Task.FromResult(tavoli);
        }

        public Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync()
        {
            var tavoli = _tavoli.Values.Where(t => t.Disponibile);
            return Task.FromResult(tavoli);
        }

        public Task<bool> NumeroExistsAsync(int numero, int? excludeId = null)
        {
            var exists = _tavoli.Values.Any(t =>
                t.Numero == numero && (!excludeId.HasValue || t.TavoloId != excludeId.Value));
            return Task.FromResult(exists);
        }

        public Task<bool> QrCodeExistsAsync(string qrCode, int? excludeId = null)
        {
            var exists = _tavoli.Values.Any(t =>
                t.QrCode == qrCode && (!excludeId.HasValue || t.TavoloId != excludeId.Value));
            return Task.FromResult(exists);
        }

        public Task UpdateAsync(TavoloDTO tavoloDto)
        {
            if (_tavoli.ContainsKey(tavoloDto.TavoloId))
            {
                _tavoli[tavoloDto.TavoloId] = tavoloDto;
            }
            return Task.CompletedTask;
        }
    }
}