using System.Net;

namespace FitupProject.BLL.Commons.Exceptions
{
    public class ExceptionHandler : Exception
    {
        public ExceptionHandler(string message) : base(message) { }
    }
}
