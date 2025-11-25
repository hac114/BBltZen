using DTO;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IScontrinoRepository
    {
        Task<ScontrinoDTO> GeneraScontrinoCompletoAsync(int ordineId);
        Task<bool> EsisteOrdineAsync(int ordineId);
        Task<bool> VerificaStatoOrdinePerScontrinoAsync(int ordineId);
    }
}