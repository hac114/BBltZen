using DTO;

namespace Repository.Interface
{
    public interface IStripeServiceRepository
    {
        Task<StripePaymentResponseDTO> CreatePaymentIntentAsync(StripePaymentRequestDTO request);
        Task<bool> ConfirmPaymentAsync(string paymentIntentId);
        Task<bool> HandleWebhookAsync(string json, string signature);
        Task<bool> RefundPaymentAsync(string paymentIntentId);
    }
}