using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace NetServNamingService
{
    [RoutePrefix("ns")]
    public class NsController : ApiController
    {
        [HttpPost]
        [Route("MasterDead")]
        public Task<bool> MasterDead([FromBody]MasterDetails master)
        {
            lock (Collections.MastersLock)
            {
                var mst = Collections.Masters.FirstOrDefault(m => m.Id == master.Id);
                if (mst != null)
                {
                    mst.IsAlive = false;
                }
            }
            return Task.FromResult(true);
        }

        [HttpPost]
        [Route("RegisterMaster")]
        public Task<bool> RegisterMaster([FromBody]MasterDetails master)
        {
            lock (Collections.MastersLock)
            {
                var mst = Collections.Masters.FirstOrDefault(m => m.Id == master.Id);
                if (mst != null)
                {
                    mst.IsAlive = true;
                }
                else
                {
                    Collections.Masters.Add(master);
                }
            }
            return Task.FromResult(true);
        }

        [HttpGet]
        [Route("GetLiveMaster")]
        public Task<MasterDetails> GetLiveMaster()
        {
            MasterDetails master = null;
            lock (Collections.MastersLock)
            {
                master = Collections.Masters.FirstOrDefault(m => m.IsAlive);
            }
            return Task.FromResult(master);
        }

        [HttpGet]
        [Route("MasterElectionStarted")]
        public Task<bool> MasterElectionStarted()
        {
            lock (Collections.MastersLock)
            {
                Collections.MasterElectionStarted = true;
            }
            return Task.FromResult(true);
        }

        [HttpGet]
        [Route("IsMasterElectionStarted")]
        public Task<bool> IsMasterElectionStarted()
        {
            return Task.FromResult(Collections.MasterElectionStarted);
        }
    }
}
