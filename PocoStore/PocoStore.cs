using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fastnet.Core.Web
{
    //class T1
    //{
    //    public int SomeNumber { get; set; }
    //    public string SomeText { get; set; }
    //}
    /// <summary>
    /// simple persistent json store for poco objects - add to IServiceCollection as a singleton
    /// </summary>
    public class PocoStore
    {
        private readonly object sentinel;
        private readonly string webRoot;
        private readonly string storeFolderPath;
        private readonly string jsonFile;
        private readonly ILogger log;
        private readonly PocoStoreConfiguration config;
        private ConcurrentDictionary<string, List<object>> objects;
        private readonly JsonSerializerSettings jsonSettings;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        //public PocoStore(IHostingEnvironment env, ILogger<PocoStore> logger, IOptions<PocoStoreConfiguration> options)
        public PocoStore(IWebHostEnvironment env, ILogger<PocoStore> logger, IOptions<PocoStoreConfiguration> options)
        {
            this.sentinel = new object();
            this.log = logger;
            this.config = options.Value;
            jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            webRoot = env.WebRootPath;
            storeFolderPath = Path.Combine(webRoot, config.StoreFolder);
            if (!Directory.Exists(storeFolderPath))
            {
                Directory.CreateDirectory(storeFolderPath);
            }
            jsonFile = Path.Combine(storeFolderPath, "PocoStore.json");
            LoadStore();
        }
        /// <summary>
        /// Add an instance to the store. Use Save() to save the store unless all instance implement INotifyPropertyChanged
        /// </summary>
        /// <param name="instance"></param>
        public void AddInstance(object instance)
        {
            Write(instance);
            Save();
        }
        /// <summary>
        /// Save the store to disk. Default location is (webroot)/data/pocostore.json
        /// </summary>
        public void Save()
        {
            SaveStore();
        }
        //private void TestWrite()
        //{
        //    var t1 = new T1 { SomeNumber = 42, SomeText = "asim shah" };
        //    Write(t1);
        //    SaveStore();

        //}
        private void SaveStore()
        {
            lock (sentinel)
            {
                var json = JsonConvert.SerializeObject(objects, Formatting.Indented, jsonSettings);
                File.WriteAllText(jsonFile, json);
            }
        }
        private void LoadStore()
        {
            lock (sentinel)
            {
                if (File.Exists(jsonFile))
                {
                    var json = File.ReadAllText(jsonFile);
                    objects = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<object>>>(json, jsonSettings);
                }
                else
                {
                    objects = new ConcurrentDictionary<string, List<object>>();
                }
            }
        }
        private void Write(object instance)
        {
            bool failure = false;
            var key = instance.GetType().FullName;
            if (!objects.ContainsKey(key))
            {
                failure = !objects.TryAdd(key, new List<object>());
                if (failure)
                {
                    log.Error($"Error creating in-memory store for {key}");
                }
            }
            if (!failure)
            {
                objects.TryGetValue(key, out List<object> list);
                if (list.Any(x => Object.ReferenceEquals(instance, x)))
                {
                    if (config.ThrowOnException)
                    {
                        throw new Exception($"key: {key}, cannot write duplicate instance");
                    }
                    else
                    {
                        log.Error($"key: {key}, cannot write duplicate instance");
                    }
                }
                else
                {
                    list.Add(instance);
                    if (instance is System.ComponentModel.INotifyPropertyChanged)
                    {
                        (instance as System.ComponentModel.INotifyPropertyChanged).PropertyChanged += Instance_PropertyChanged;
                    }
                }
            }
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
        }
    }
}
