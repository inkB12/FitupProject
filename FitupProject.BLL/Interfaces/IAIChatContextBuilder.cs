namespace FitupProject.BLL.Interfaces
{
    public interface IAIChatContextBuilder
    {
        Task<string> BuildContextAsync(string accountId);
    }
}
