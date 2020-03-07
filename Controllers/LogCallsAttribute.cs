using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fastnet.Core.Web.Controllers
{
    /// <summary>
    /// adds logging of all calls into a controller
    /// </summary>
    public class LogCallsAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exclusionList">array of method names that will not be logged</param>
        public LogCallsAttribute(params string[] exclusionList) : base(typeof(LogCallsImplementation))
        {
            //this.exclusionList = exclusionList;
            this.Arguments = new object[] { exclusionList };
        }
        private class LogCallsImplementation : IAsyncActionFilter, IAsyncResultFilter
        {
            private ILogger log;
            private string[] exclusionList;
            public LogCallsImplementation(ILogger<LogCallsAttribute> log, object[] arguments)
            {
                this.log = log;
                if (arguments != null && arguments.Length > 0)
                {
                    exclusionList = (string[])arguments;
                }
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                (bool canLog, string actionName) r = CanLog(context.ActionDescriptor);
                //var nameMethod = context.ActionDescriptor.GetType().GetMethod("get_ActionName");
                //string name = (string)nameMethod.Invoke(context.ActionDescriptor, null);
                if (r.canLog)
                //if (exclusionList == null || exclusionList.Length == 0 || !exclusionList.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                {
                    var remoteName = context.HttpContext.GetRemoteName();
                    var t = context.ActionArguments.Select(args => $"{args.Key}={args.Value.ToString()}");
                    var text = $"{context.Controller.GetType().Name} called (from {remoteName}) with {context.HttpContext.Request.Path.Value} --> {r.actionName}({(string.Join(", ", t.ToArray()))})";
                    log.Information(text);
                }
                await next();
            }
            public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
            {
                (bool canLog, string actionName) r = CanLog(context.ActionDescriptor);
                if (r.canLog)
                {
                    var dr = (context.Result as ObjectResult)?.Value as DataResult;
                    string returned = dr?.ToString();
                    //var text = $"{context.Controller.GetType().Name} called with {context.HttpContext.Request.Path.Value} <-- {r.actionName}() returned {(returned ?? context.Result.ToString())}";
                    var text = $"{context.Controller.GetType().Name} returned {(returned ?? context.Result.ToString())} from {r.actionName}() <-- {context.HttpContext.Request.Path.Value}";
                    log.Information(text);
                }
                await next();
            }

            private (bool, string) CanLog(ActionDescriptor actionDescriptor)
            {
                var nameMethod = actionDescriptor.GetType().GetMethod("get_ActionName");
                string name = (string)nameMethod.Invoke(actionDescriptor, null);
                bool canLog = exclusionList == null || exclusionList.Length == 0 || !exclusionList.Contains(name, StringComparer.CurrentCultureIgnoreCase);
                return (canLog, name);
            }
        }
    }
}
