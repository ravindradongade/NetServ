using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity.Enums
{
    public enum MessageType
    {
        HealthInfo=1,
        DeclareDead=2,
        Election=3,
        DataMessage=4,
        Broadcast=5

    }
}
