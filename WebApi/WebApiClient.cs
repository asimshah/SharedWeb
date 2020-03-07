using Fastnet.Core.Web.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fastnet.Core.Web
{
    internal class JsonContent : StringContent
    {
        public JsonContent(object obj) : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {

        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class WebApiClient : HttpClient
    {
        /// <summary>
        /// 
        /// </summary>
        protected ILogger log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        internal WebApiClient(string url)
        {
            BaseAddress = new Uri(url);
            DefaultRequestHeaders.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="logger"></param>
        public WebApiClient(string url, ILogger logger) : this(url)
        {
            this.log = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="loggerFactory"></param>
        public WebApiClient(string url, ILoggerFactory loggerFactory) : this(url)
        {
            this.log = loggerFactory.CreateLogger(this.GetType().FullName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<T> GetDataResultObject<T>(string query)
        {
            var response = await GetAsync(query);
            if (response != null)
            {
                var json = await response.Content.ReadAsStringAsync();
                DataResult dr = JsonConvert.DeserializeObject<DataResult>(json);
                if (dr.Success)
                {
                    if (dr.Data is T)
                    {
                        return (T)dr.Data;
                    }
                    else
                    {
                        object data = dr.Data;
                        return JsonConvert.DeserializeObject<T>(data.ToString());
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(dr.ExceptionMessage))
                    {
                        log.Error($"{this.BaseAddress}/{query} failed with server exception: {dr.ExceptionMessage}");
                    }
                    else
                    {
                        log.Warning($"{this.BaseAddress}/{query} failed with message: {(dr.Message ?? "(no message)")}");
                    }
                }
                return default(T);
            }
            return default(T);
        }
        private new async Task<HttpResponseMessage> GetAsync(string query)
        {
            try
            {
                var response = await base.GetAsync(query);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = $"{this.BaseAddress}/{query} failed with status code: {response.StatusCode}";
                    log.Error(msg);
                }
                return response;
            }
            catch (Exception xe)
            {
                log.Error($"{this.BaseAddress}/{query} failed with exception: {xe.Message}");
                //throw;
            }
            return null;
        }
        /// <summary>
        /// Post an object without any returing data
        /// Can be used with both fastnet and external api's
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual async Task PostAsync<T>(string query, T obj)
        {
            try
            {
                var content = new JsonContent(obj);
                var response = await base.PostAsync(query, content);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = $"Post: {this.BaseAddress}{query} failed with status code: {response.StatusCode}";
                    log.Error(msg);
                }
                //return response;
            }
            catch (Exception xe)
            {
                log.Error($"Post: {this.BaseAddress}{query} failed with exception: {xe.Message}");
                //throw;
            }
        }
        /// <summary>
        /// Post an object of type ST and return with an object of type RT
        /// Can be used with external api's or any that do not return using DataResult
        /// </summary>
        /// <typeparam name="ST"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual async Task<RT> PostJsonAsync<ST, RT>(string query, ST obj)
        {
            try
            {
                var content = new JsonContent(obj);
                var response = await base.PostAsync(query, content);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = $"Post: {this.BaseAddress}{query} failed with status code: {response.StatusCode}";
                    log.Error(msg);
                    return default(RT);
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RT>(json);
                }
                //return response;
            }
            catch (Exception xe)
            {
                log.Error($"Post: {this.BaseAddress}{query} failed with exception: {xe.Message}");
                //throw;
            }
            return default(RT);
        }
        /// <summary>
        /// Post an object of type ST and return with an object of type RT
        /// Can be used with fastnet api's as returned data has to be DataResult
        /// </summary>
        /// <typeparam name="ST"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual async Task<RT> PostAsync<ST, RT>(string query, ST obj)
        {
            try
            {
                var content = new JsonContent(obj);
                var response = await base.PostAsync(query, content);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = $"Post: {this.BaseAddress}{query} failed with status code: {response.StatusCode}";
                    log.Error(msg);
                    return default(RT);
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    DataResult dr = JsonConvert.DeserializeObject<DataResult>(json);
                    if (dr.Success)
                    {
                        if (dr.Data is RT)
                        {
                            return (RT)dr.Data;
                        }
                        else
                        {
                            object data = dr.Data;
                            return JsonConvert.DeserializeObject<RT>(data.ToString());
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(dr.ExceptionMessage))
                        {
                            log.Error($"{this.BaseAddress}/{query} failed with server exception: {dr.ExceptionMessage}");
                        }
                        else
                        {
                            log.Warning($"{this.BaseAddress}/{query} failed with message: {(dr.Message ?? "(no message)")}");
                        }
                    }
                    return default(RT);
                }
                //return response;
            }
            catch (Exception xe)
            {
                log.Error($"Post: {this.BaseAddress}{query} failed with exception: {xe.Message}");
                //throw;
            }
            return default(RT);
        }
    }
}
