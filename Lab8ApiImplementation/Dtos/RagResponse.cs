namespace Lab8ApiImplementation.Dtos
{
    // What POST /api/rag/ask sends back
    public class RagResponse
    {
        public string Answer { get; set; } = "";
        public List<RagSource> Sources { get; set; } = new List<RagSource>();

        // All zeros when nothing was found, because Groq was not called
        public ChatUsage Usage { get; set; } = new ChatUsage();
    }
}
