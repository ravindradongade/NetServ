using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServData
{
    public class MasterDetails
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public int Port { get; set; }
        public bool IsAlive { get; set; }

        public string DeadDeclairedDate { get; set; }

    }
}
