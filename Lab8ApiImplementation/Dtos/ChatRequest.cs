namespace Lab8ApiImplementation.Dtos
{
    // What the client sends to POST /api/chat
    // The model has no memory, so the client sends the whole conversation every time
    public class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
