using NetServNodeEntity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServData
{
    public class DataFactory
    {
        public static IDataManager GetDataManager(StorageType storageType, string connectionString="")
        {
            switch (storageType)
            {
                case StorageType.SQL:
                    return new SqlDataManager(connectionString);
                    break;
                case StorageType.NamingService:
                    return new NSManager();
                default:
                    return null;
                    break;
            }
        }
    }
}
