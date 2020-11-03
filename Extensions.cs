using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    public enum Browsers
    {
        Unknown,
        Edge,
        IE,
        Chrome,
        Firefox,
        Safari
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        //regex from http://detectmobilebrowsers.com/
        // b2 is a version of b that includes ipad
        private static readonly Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        //private static readonly Regex b2 = new Regex(@"(android|ipad|playbook|silk|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        /// <summary>
        /// true if requesting device is a mobile device
        /// note 'matchMedia("(hover: none) and (pointer: coarse)")' in the client is a better choice
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsMobileBrowser(this HttpRequest request)
        {
            return request.UserAgent().IsMobileBrowser();
        }
        /// <summary>
        /// true if requesting device is an ipad
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsIpad(this HttpRequest request)
        {
            return request.UserAgent().IsIpad();
        }
        /// <summary>
        /// Get ipaddress of the remote browser
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetRemoteIPAddress(this HttpContext ctx)
        {
            return ctx.Connection.RemoteIpAddress.ToString();
        }
        /// <summary>
        /// Get the browser making the request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Browsers GetBrowser(this HttpRequest request)
        {
            return request.UserAgent().ParseUserAgentForBrowser();
        }
        /// <summary>
        /// true if user agent string is for a mobile device
        /// note 'matchMedia("(hover: none) and (pointer: coarse)")' in the client is a better choice
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static bool IsMobileBrowser(this string userAgent)
        {
            //var userAgent = request.UserAgent();
            if (userAgent != null && ((b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, 4)))))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// true if requesting device is an ipad
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static bool IsIpad(this string userAgent)
        {
            //var userAgent = request.UserAgent();
            if (userAgent != null && userAgent.ToLower().Contains("ipad"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string UserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"];
        }
        /// <summary>
        /// Parsea user agent string and identify the browser
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static Browsers ParseUserAgentForBrowser(this string userAgent)
        {
            Browsers browser = Browsers.Unknown;
            if (userAgent != null)
            {
                var text = userAgent.ToLower();
                if (text.Contains("edge"))
                {
                    browser = Browsers.Edge;
                }
                else if (text.Contains("chrome"))
                {
                    browser = Browsers.Chrome;
                }
                else if (text.Contains("safari"))
                {
                    browser = Browsers.Safari;
                }
                else if (text.Contains("firefox"))
                {
                    browser = Browsers.Firefox;
                }
                else if (text.Contains("msie"))
                {
                    browser = Browsers.IE;
                }
                else if (text.Contains("iemobile"))
                {
                    browser = Browsers.IE;
                }
            }
            return browser;
        }
        /// <summary>
        /// Converts an ip address so that it can be compared to another ip address
        /// Note: this method removes the scope part of an IP V6 address
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static string GetComparableAddress(this IPAddress addr)
        {
            var text = addr.ToString();
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                text = text.Substring(0, text.IndexOf("%"));
            }
            return text;
        }
        /// <summary>
        /// attempts to determine a name for the current remote ip address
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetRemoteName(this HttpContext ctx)
        {
            string name = null;
            IPHostEntry entry = null;
            var remoteIp = ctx.Connection.RemoteIpAddress;
            try
            {
                entry = Dns.GetHostEntry(remoteIp);
            }
            catch { }
            if (entry == null || string.IsNullOrWhiteSpace(entry.HostName))
            {
                name = remoteIp.ToString();
            }
            else
            {
                name = entry.HostName.ToLower();
            }
            return name;
        }
        /// <summary>
        /// get instance of Type T from the request body
        /// </summary>
        /// <remarks>
        /// Designed for use with web api posts that are called with complex types. An alternative to providing
        /// model binders and providers for all the types (remember there may be nested types) is to post the data as a json string
        /// (remembering to ensure that content-type application/json is present in the header). There is then no need for a [FromBody] parameter
        /// as this method pulls the json string out and converts it using ToInstance()
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<T> FromBody<T>(this HttpRequest request)
        {
            async Task<string> extractBodyjson()
            {
                //request.EnableRewind();
                request.EnableBuffering();
                var stream = request.Body;
                stream.Position = 0;
                using (var sr = new StreamReader(stream))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            var json = await extractBodyjson();

            return json.ToInstance<T>();
        }
        /// <summary>
        /// modifies a connection string containing |DataDirectory| to use the Data folder of the contentRoot
        /// and attaches "-dev" to the databasename if in a development environment
        /// </summary>
        /// <param name="env"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static string LocaliseConnectionString(this IWebHostEnvironment env, string cs)
        {
            var dataDirectory = Path.Combine(env.ContentRootPath, "Data");
            if (dataDirectory.CanAccess(true, true))
            {
                SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder(cs);
                cb.AttachDBFilename = cb.AttachDBFilename.Replace("|DataDirectory|", Path.Combine(env.ContentRootPath, "Data"));
                if (env.IsDevelopment())
                {
                    cb.InitialCatalog = $"{cb.InitialCatalog}-dev";
                }
                return cb.ToString();
            }
            else
            {
                throw new Exception($"cannot access {dataDirectory}");
            }
        }
    }
}
