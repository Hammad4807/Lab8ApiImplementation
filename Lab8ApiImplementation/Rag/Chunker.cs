namespace Lab8ApiImplementation.Rag
{
    // Cuts a long text into overlapping pieces
    public static class Chunker
    {
        public static List<string> Split(string text, int size, int overlap)
        {
            if (overlap >= size)
            {
                // Otherwise "start" would never move forward and the loop would never end
                throw new ArgumentException("Overlap must be smaller than the chunk size.");
            }

            var pieces = new List<string>();

            // Each new chunk starts (size - overlap) characters after the previous one,
            // so the last 'overlap' characters of a chunk are repeated at the start of the next
            for (int start = 0; start < text.Length; start += size - overlap)
            {
                int length = Math.Min(size, text.Length - start);
                string piece = text.Substring(start, length).Trim();

                if (piece.Length > 0)
                {
                    pieces.Add(piece);
                }

                // This chunk already reached the end of the text, so stop
                if (start + length >= text.Length)
                {
                    break;
                }
            }

            return pieces;
        }
    }
}
