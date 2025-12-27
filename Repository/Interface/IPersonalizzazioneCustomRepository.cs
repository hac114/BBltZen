using DTO;

namespace Repository.Interface
{
    public interface IPersonalizzazioneCustomRepository
    {
        Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<PersonalizzazioneCustomDTO>> GetByIdAsync(int persCustomId);
        Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetBicchiereByIdAsync(int bicchiereId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetByGradoDolcezzaAsync(byte gradoDolcezza, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetBicchiereByDescrizioneAsync(string  descrizioneBicchiere, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetByNomeAsync(string nome, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<PersonalizzazioneCustomDTO>> AddAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int persCustomId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int persCustomId);

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountBicchiereByDescrizioneAsync(string descrizioneBicchiere);
        Task<SingleResponseDTO<int>> CountByGradoDolcezzaAsync(byte gradoDolcezza);
    }
}