using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Services
{
    public interface IChatService
    {
        // Sends the conversation to the AI and returns its reply
        Task<ChatResponse> SendAsync(List<ChatMessage> messages);
    }
}
