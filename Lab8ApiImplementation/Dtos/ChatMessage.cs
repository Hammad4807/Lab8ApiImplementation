namespace Lab8ApiImplementation.Dtos
{
    // One message in the conversation
    // Role is "user" (the person), "assistant" (the AI) or "system" (instructions for the AI, added by the server only)
    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
