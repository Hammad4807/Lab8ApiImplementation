using System.Net.Http.Headers;
using System.Text.Json;
using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Services
{
    public class GroqChatService : IChatService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GroqChatService> _logger;

        // Settings come from appsettings.json ("Groq" section), so changing the model needs no code change
        private readonly string _model;
        private readonly int _maxCompletionTokens;
        private readonly string _systemPrompt;
        private readonly string _apiKey;

        // HttpClient is given to us by IHttpClientFactory (registered in Program.cs)
        public GroqChatService(HttpClient http, IConfiguration config, ILogger<GroqChatService> logger)
        {
            _http = http;
            _logger = logger;

            _model = config["Groq:Model"] ?? "openai/gpt-oss-120b";
            _maxCompletionTokens = int.Parse(config["Groq:MaxCompletionTokens"] ?? "300");
            _systemPrompt = config["Groq:SystemPrompt"] ?? "You are a helpful assistant.";

            // Read the key from user-secrets or the GROQ_API_KEY environment variable - never from code
            _apiKey = config["GROQ_API_KEY"] ?? "";

            // STEP 1: set up the client
            _http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ChatResponse> SendAsync(List<ChatMessage> messages, string? systemPrompt = null, double? temperature = null)
        {
            // Checked here, not in the constructor: the constructor runs before validation,
            // so throwing there would turn every 400 validation error into a 500
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("GROQ_API_KEY is not set. Use Manage User Secrets to set it.");
            }

            // The system prompt always goes first, then the conversation.
            // ?? = use the caller's prompt if given, otherwise the normal chat prompt from appsettings.json
            var allMessages = new List<ChatMessage>();
            allMessages.Add(new ChatMessage { Role = "system", Content = systemPrompt ?? _systemPrompt });
            allMessages.AddRange(messages);

            var body = new
            {
                model = _model,
                max_completion_tokens = _maxCompletionTokens,
                // 1.0 is Groq's default; RAG passes a low value (0.2) so answers stick to the facts
                temperature = temperature ?? 1.0,
                //// gpt-oss "thinks" before answering, and thinking uses tokens too - keep it short
                //reasoning_effort = "low",
                messages = allMessages
            };

            // STEP 2: send the prompt
            HttpResponseMessage res = await _http.PostAsJsonAsync("chat/completions", body);

            if (!res.IsSuccessStatusCode)
            {
                string errorBody = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("Groq returned {StatusCode}: {ErrorBody}", (int)res.StatusCode, errorBody);

                // ErrorHandlingMiddleware will catch this and return a 500
                throw new HttpRequestException($"Groq request failed with status {(int)res.StatusCode}.");
            }

            // STEP 3: read the answer
            string json = await res.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            string reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            ChatUsage usage = doc.RootElement
                .GetProperty("usage")
                .Deserialize<ChatUsage>() ?? new ChatUsage();

            // Log token usage to keep an eye on cost
            _logger.LogInformation("Groq used {PromptTokens} prompt + {CompletionTokens} completion = {TotalTokens} tokens",
                usage.PromptTokens, usage.CompletionTokens, usage.TotalTokens);

            return new ChatResponse
            {
                Reply = reply,
                Usage = usage
            };
        }
    }
}
