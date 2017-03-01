namespace NetServNamingService
{
    public class MasterDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public int Port { get; set; }
        public bool IsAlive { get; set; }

        public string DeadDeclairedDate { get; set; }
    }
}
