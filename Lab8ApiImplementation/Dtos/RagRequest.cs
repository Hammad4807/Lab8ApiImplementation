namespace Lab8ApiImplementation.Dtos
{
    // What the client sends to POST /api/rag/ask
    public class RagRequest
    {
        public string Question { get; set; } = "";
    }
}
