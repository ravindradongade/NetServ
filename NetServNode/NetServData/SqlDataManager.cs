using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetServData
{
    public class SqlDataManager : IDataManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SqlDataManager));
        private readonly string _connectionString;
        public SqlDataManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async void DeclareMasterDead(MasterDetails node)
        {
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    node.IsAlive = false;
                    node.DeadDeclairedDate = DateTime.UtcNow.ToShortTimeString();
                    context.MasterDetail.Add(node);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

        }

        public async void DeclareNodeDead(NodeDetails node)
        {
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    node.IsAlive = false;
                    node.DeadDeclairedDate = DateTime.UtcNow.ToShortTimeString();
                    context.NodeDetails.Add(node);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

        }

        public List<NodeDetails> GetAllNodes()
        {
            List<NodeDetails> nodes = null;
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    nodes = context.NodeDetails.ToList();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return nodes;
        }

        public MasterDetails GetLiveMasterDetails()
        {
            MasterDetails master = null;
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    master = context.MasterDetail.FirstOrDefault(m => m.IsAlive);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return master;
        }

        public List<NodeDetails> GetLiveNodes()
        {
            List<NodeDetails> nodes = null;
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    nodes = context.NodeDetails.Where(n => n.IsAlive).ToList();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return nodes;
        }

        public NodeDetails GetNode(string nodeId)
        {
            NodeDetails node = null;
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    node = context.NodeDetails.FirstOrDefault(n => n.IsAlive);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return node;
        }

        public async void InsertMaster(MasterDetails master)
        {
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    context.MasterDetail.Add(master);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public async void InsertNode(NodeDetails node)
        {
            try
            {
                using (var context = new SqlDataModel(_connectionString))
                {
                    context.NodeDetails.Add(node);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}
