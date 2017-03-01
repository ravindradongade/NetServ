using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServData
{
    public interface IDataManager
    {
        void InsertMaster(MasterDetails master);
        MasterDetails GetLiveMasterDetails();
        void InsertNode(NodeDetails node);
        NodeDetails GetNode(string nodeId);
        List<NodeDetails> GetLiveNodes();
        List<NodeDetails> GetAllNodes();
        void DeclareNodeDead(NodeDetails node);
        void DeclareMasterDead(MasterDetails node);
       
    }
}
