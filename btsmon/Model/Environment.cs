using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class Environment
    {
        [DataMember]
        public String GroupServer { get; set; }
        [DataMember]
        public String GroupInstance { get; set; }
        [DataMember]
        public HostInstance[] HostInstances { get; set; }
        [DataMember]
        public Server[] Servers { get; set; }
    }
}