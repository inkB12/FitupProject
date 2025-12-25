namespace FitupProject.BLL.Commons.Helpers
{
    public static class EmailTemplate
    {
        public static string LoadOtpTemplate(string otp, int minutes = 30)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "OtpEmail.html");
            var html = File.ReadAllText(path);

            return html.Replace("{{OTP}}", otp)
                       .Replace("{{MINUTES}}", minutes.ToString());
        }
    }
}
