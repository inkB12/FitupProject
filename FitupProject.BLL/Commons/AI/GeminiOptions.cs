namespace FitupProject.BLL.Commons.AI
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
        public string Model { get; set; } = "gemini-2.5-flash-lite";

        public int MaxOutputTokens { get; set; } = 300;
        public double Temperature { get; set; } = 0.3;
        public int HistoryMessageLimit { get; set; } = 10;
        public bool EnableLiveContext { get; set; } = true;

        // để test khi chưa có API key thật
        public bool UseMockResponse { get; set; } = false;
    }
}
