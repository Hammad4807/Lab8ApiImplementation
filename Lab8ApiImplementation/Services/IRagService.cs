using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Services
{
    public interface IRagService
    {
        // Answers a question using only the documents in the knowledge base
        Task<RagResponse> AskAsync(string question);
    }
}
