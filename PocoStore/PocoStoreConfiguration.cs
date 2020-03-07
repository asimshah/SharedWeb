namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class PocoStoreConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public string StoreFolder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ThrowOnException { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PocoStoreConfiguration()
        {
            StoreFolder = "Data";
            ThrowOnException = false;
        }
    }
}
