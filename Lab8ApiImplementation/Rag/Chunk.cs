namespace Lab8ApiImplementation.Rag
{
    // One small piece of a document, plus the file it came from (used for citations)
    public class Chunk
    {
        public string Source { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
