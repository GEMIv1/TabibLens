using Domain.Interfaces;
using Infra.ExternalApis.Groq;
using Infra.ExternalApis.HuggingFace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.ExternalApis
{
    public static class ExternalApiRegistration
    {

        public static IServiceCollection AddExternalApis(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // QWEN OCR via Hugging Face
            services.Configure<HuggingFaceOptions>(
                configuration.GetSection(HuggingFaceOptions.SectionName));
            services.AddHttpClient<IOcrService, QwenOcrService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(120); // OCR can take time for large images
            });

            // Groq Chat
            services.Configure<GroqOptions>(
                configuration.GetSection(GroqOptions.SectionName));
            services.AddHttpClient<IChatAiService, GroqChatService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
