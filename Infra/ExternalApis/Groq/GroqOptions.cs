namespace Infra.ExternalApis.Groq
{
    public class GroqOptions
    {
        public const string SectionName = "Groq";
        public required string ApiKey { get; set; }
        public string Model { get; set; } = "llama-3.3-70b-versatile";
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1024;
    }
}
