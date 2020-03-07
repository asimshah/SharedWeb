using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace Fastnet.Core.Web.Controllers
{
    /// <summary>
    /// provides features such as cached responses and DataResult
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// current user agent string
        /// </summary>
        protected string userAgent;
        /// <summary>
        /// current client browser
        /// </summary>
        protected Browsers browser;
        protected readonly IWebHostEnvironment environment;
        /// <summary>
        /// 
        /// </summary>
        protected ILogger log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="env"></param>
        protected BaseController(ILogger logger, IWebHostEnvironment env)
        {
            this.log = logger;
            this.environment = env;
        }
        /// <summary>
        /// 
        /// </summary>
        protected bool IsAuthenticated
        {
            get
            {
                return User.Identity.IsAuthenticated;
            }
        }
        /// <summary>
        /// Returns a success with data - data can be null
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected IActionResult SuccessResult(object data = null)
        {
            DataResult dr = new DataResult { Success = true, Data = data };
            return new ObjectResult(dr);
        }
        /// <summary>
        /// returns an error/failure with a message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected IActionResult ErrorResult(string message)
        {
            DataResult dr = new DataResult { Success = false, Data = null, Message = message };
            return new ObjectResult(dr);
        }
        /// <summary>
        /// Returns an exception with message - defaults to "System Error"
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected IActionResult ExceptionResult(Exception xe, string message = "System Error")
        {
            DataResult dr = new DataResult { Success = false, Data = null, ExceptionMessage = xe.Message, Message = message };
            return new ObjectResult(dr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected IActionResult CacheableResult(IActionResult value, params object[] args)
        {
            var etagStatus = IsEtagUnchanged(args);
            if (etagStatus.IsUnchanged)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
            {
                Public = true
            };
            Response.GetTypedHeaders().ETag = new EntityTagHeaderValue(etagStatus.NewEtag);
            return value;
            //return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected (bool IsUnchanged, string NewEtag) IsEtagUnchanged(params object[] args)
        {
            var newEtag = CreateEtag(args);
            var ifNoneMatch = Request.GetTypedHeaders().IfNoneMatch;
            var receivedETag = ifNoneMatch?.FirstOrDefault()?.Tag.Value;
            return (receivedETag == newEtag, newEtag);
        }
        private string CreateEtag(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            void addItemEtag(object item)
            {
                if (item != null)
                {
                    if (!(item is string) && item is IEnumerable)
                    {
                        var subargs = item as IEnumerable;
                        foreach (var subarg in subargs)
                        {
                            addItemEtag(subarg);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0:x}", item.GetHashCode());
                    }
                }

            }
            foreach (object arg in args)
            {
                addItemEtag(arg);
            }
            string etag = "\"" + sb.ToString() + "\"";
            return etag;
        }
    }
}
