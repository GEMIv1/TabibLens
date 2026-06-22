namespace Infra.ExternalApis.HuggingFace
{
    public class HuggingFaceOptions
    {
        public const string SectionName = "HuggingFace";
        public required string ApiKey { get; set; }
        public required string ModelId { get; set; }
        public string BaseUrl { get; set; } = "https://router.huggingface.co/hf-inference/models";
    }
}
