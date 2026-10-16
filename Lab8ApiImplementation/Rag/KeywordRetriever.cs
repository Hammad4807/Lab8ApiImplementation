using System.Text.RegularExpressions;

namespace Lab8ApiImplementation.Rag
{
    // A chunk together with how well it matches the question
    public class ScoredChunk
    {
        public Chunk Chunk { get; set; } = new Chunk();
        public double Score { get; set; }
    }

    // RETRIEVE: finds the chunks that share the most (and the rarest) words with the question
    public static class KeywordRetriever
    {
        // Common words that appear everywhere and say nothing about the topic
        private static readonly string[] StopWords =
        {
            "the", "and", "for", "are", "what", "how", "does", "you", "your", "with", "can"
        };

        public static List<ScoredChunk> TopK(List<Chunk> chunks, string question, int k)
        {
            List<string> questionWords = Words(question).Distinct().ToList();

            // Split every chunk into words once
            var chunkWords = new List<List<string>>();
            foreach (Chunk chunk in chunks)
            {
                chunkWords.Add(Words(chunk.Text));
            }

            var results = new List<ScoredChunk>();

            for (int i = 0; i < chunks.Count; i++)
            {
                double score = 0;

                foreach (string word in questionWords)
                {
                    // How many times the word appears in THIS chunk
                    int countInChunk = chunkWords[i].Count(w => w == word);

                    // How many chunks contain the word at all
                    int chunksWithWord = chunkWords.Count(words => words.Contains(word));

                    // A rare word (in few chunks) is worth more than a word that is everywhere
                    double rarity = Math.Log(1 + (double)chunks.Count / (1 + chunksWithWord));

                    score += countInChunk * rarity;
                }

                if (score > 0)
                {
                    results.Add(new ScoredChunk { Chunk = chunks[i], Score = score });
                }
            }

            // Best matches first, keep only the top k
            return results
                .OrderByDescending(r => r.Score)
                .Take(k)
                .ToList();
        }

        // "How much is Express shipping?" -> ["much", "express", "shipping"]
        private static List<string> Words(string text)
        {
            return Regex.Matches(text.ToLowerInvariant(), "[a-z0-9]{3,}")
                .Select(m => m.Value)
                .Where(w => !StopWords.Contains(w))
                .ToList();
        }
    }
}
