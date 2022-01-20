namespace ShareOut.Internal
{
    public class RedisOptions
    {
        public string[] Connections { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public int AsyncTimeout { get; set; }
        public int ConnectRetry { get; set; }
        public bool Ssl { get; set; }
        public string Password { get; set; }
        public string Instance { get; set; }
    }
}
