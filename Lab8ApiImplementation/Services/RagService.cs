using Lab8ApiImplementation.Dtos;
using Lab8ApiImplementation.Rag;

namespace Lab8ApiImplementation.Services
{
    // RAG = Retrieve the relevant chunks, Augment the prompt with them, Generate the answer
    public class RagService : IRagService
    {
        private const string NotFoundAnswer =
            "I do not know. I could not find anything about that in the documents.";

        private readonly IChatService _chatService;
        private readonly DocumentStore _store;
        private readonly ILogger<RagService> _logger;

        private readonly int _topK;
        private readonly double _temperature;
        private readonly string _systemPrompt;

        public RagService(IChatService chatService, DocumentStore store, IConfiguration config, ILogger<RagService> logger)
        {
            _chatService = chatService;
            _store = store;
            _logger = logger;

            // GetValue converts the text from appsettings.json to the right type (and reads "0.2" correctly)
            _topK = config.GetValue<int>("Rag:TopK", 3);
            _temperature = config.GetValue<double>("Rag:Temperature", 0.2);
            _systemPrompt = config["Rag:SystemPrompt"]
                ?? "Answer using ONLY the context. Cite sources like [1]. If the answer is not in the context, say you do not know.";
        }

        public async Task<RagResponse> AskAsync(string question)
        {
            // 1. RETRIEVE: the best matching chunks
            List<ScoredChunk> hits = KeywordRetriever.TopK(_store.Chunks, question, _topK);

            // Log which chunks were picked and their scores - never the question or any secrets
            foreach (ScoredChunk hit in hits)
            {
                _logger.LogInformation("RAG hit {Source} score {Score:F2}", hit.Chunk.Source, hit.Score);
            }

            // Nothing matched: answer "I do not know" without calling Groq (saves tokens and stops guessing)
            if (hits.Count == 0)
            {
                _logger.LogInformation("RAG: no relevant chunks found, Groq not called");
                return new RagResponse { Answer = NotFoundAnswer };
            }

            // 2. AUGMENT: number the chunks so the model can cite them as [1], [2], ...
            var sources = new List<RagSource>();
            for (int i = 0; i < hits.Count; i++)
            {
                sources.Add(new RagSource
                {
                    Number = i + 1,
                    Source = hits[i].Chunk.Source,
                    Text = hits[i].Chunk.Text,
                    Score = Math.Round(hits[i].Score, 2)
                });
            }

            string context = string.Join("\n\n", sources.Select(s => $"[{s.Number}] ({s.Source})\n{s.Text}"));
            string userMessage = $"Context:\n{context}\n\nQuestion: {question}";

            // 3. GENERATE: reuse GroqChatService, but with the RAG rules and a low temperature
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = userMessage }
            };
            ChatResponse response = await _chatService.SendAsync(messages, _systemPrompt, _temperature);

            return new RagResponse
            {
                Answer = response.Reply,
                Sources = sources,
                Usage = response.Usage
            };
        }
    }
}
