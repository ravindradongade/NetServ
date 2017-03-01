using System.ComponentModel.DataAnnotations;

namespace NetServData
{
    public class NodeDetails
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string MasterSelector { get; set; }
        public bool IsAlive { get; set; }
        public string DeadDeclairedDate { get; set; }
    }
}
