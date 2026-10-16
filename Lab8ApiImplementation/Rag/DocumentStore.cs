namespace Lab8ApiImplementation.Rag
{
    // INGEST: loads every .txt file, cuts it into chunks and keeps them in memory.
    // Registered as a Singleton, so this runs only once for the whole app, not on every request.
    public class DocumentStore
    {
        public List<Chunk> Chunks { get; } = new List<Chunk>();

        public DocumentStore(IConfiguration config, ILogger<DocumentStore> logger)
        {
            string docsFolder = config["Rag:DocsFolder"] ?? "Data/Docs";
            int chunkSize = int.Parse(config["Rag:ChunkSize"] ?? "200");
            int chunkOverlap = int.Parse(config["Rag:ChunkOverlap"] ?? "40");

            // The .csproj copies the docs next to the built app (bin\Debug\net10.0\Data\Docs)
            string folderPath = Path.Combine(AppContext.BaseDirectory, docsFolder);

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"RAG docs folder not found: {folderPath}");
            }

            foreach (string file in Directory.GetFiles(folderPath, "*.txt"))
            {
                string text = File.ReadAllText(file);
                string fileName = Path.GetFileName(file);

                foreach (string piece in Chunker.Split(text, chunkSize, chunkOverlap))
                {
                    Chunks.Add(new Chunk { Source = fileName, Text = piece });
                }
            }

            logger.LogInformation("RAG: indexed {ChunkCount} chunks from {FolderPath}", Chunks.Count, folderPath);
        }
    }
}
