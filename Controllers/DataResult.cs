using Microsoft.AspNetCore.Mvc;

namespace Fastnet.Core.Web.Controllers
{
    /// <summary>
    /// wrapper for structured ActionResult containing data
    /// </summary>
    public class DataResult : ActionResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// optional message to include in the DataResult
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// excpetion message when returning an error
        /// </summary>
        public string ExceptionMessage { get; set; }
        /// <summary>
        /// the data to wrap in a DataResult
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Success)
            {
                string text = "";
                if (this.Data != null)
                {
                    text = $", {this.Data?.ToString()}{(string.IsNullOrWhiteSpace(Message) ? "" : "message=" + Message)}";
                }
                return $"{{success{text}}}";
            }
            else
            {
                return $"{{failed, {(string.IsNullOrWhiteSpace(Message) ? "" : "message=" + Message)}, {(string.IsNullOrWhiteSpace(ExceptionMessage) ? "" : "exceptionMessage=" + ExceptionMessage)} }}";
            }
        }
    }
}
