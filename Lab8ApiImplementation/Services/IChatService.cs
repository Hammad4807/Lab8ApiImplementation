using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Services
{
    public interface IChatService
    {
        // Sends the conversation to the AI and returns its reply.
        // systemPrompt / temperature are optional: leave them out to use the normal chat settings,
        // or pass them in (like RagService does) to give the AI different rules.
        Task<ChatResponse> SendAsync(List<ChatMessage> messages, string? systemPrompt = null, double? temperature = null);
    }
}
