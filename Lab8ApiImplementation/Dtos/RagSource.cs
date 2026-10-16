namespace Lab8ApiImplementation.Dtos
{
    // One passage that was put into the prompt. Number matches the [1], [2] citations in the answer.
    public class RagSource
    {
        public int Number { get; set; }
        public string Source { get; set; } = "";
        public string Text { get; set; } = "";
        public double Score { get; set; }
    }
}
