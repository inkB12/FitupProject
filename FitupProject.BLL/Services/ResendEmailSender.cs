using FitupProject.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FitupProject.BLL.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;

        public ResendEmailSender(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var apiKey = _cfg["Resend:ApiKey"];
            var from = _cfg["Resend:From"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Missing config: Resend:ApiKey");
            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("Missing config: Resend:From");

            _http.BaseAddress = new Uri("https://api.resend.com/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from,
                to = new[] { toEmail },
                subject,
                html = htmlBody
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync("emails", content);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Email provider error: {(int)resp.StatusCode} - {respBody}");
            }
        }
    }
}
