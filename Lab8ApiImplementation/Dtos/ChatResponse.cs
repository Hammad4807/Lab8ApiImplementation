using System.Text.Json.Serialization;

namespace Lab8ApiImplementation.Dtos
{
    // What POST /api/chat sends back to the client
    public class ChatResponse
    {
        public string Reply { get; set; } = "";
        public ChatUsage Usage { get; set; } = new ChatUsage();
    }

    // How many tokens the call used (tokens = cost)
    // [JsonPropertyName] keeps the snake_case names that Groq uses
    public class ChatUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
