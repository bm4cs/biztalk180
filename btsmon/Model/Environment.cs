using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class Environment
    {

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String GroupServer { get; set; }

        [DataMember]
        public String GroupInstance { get; set; }

        [DataMember]
        public String MgmtDatabase { get; set; }

        [DataMember]
        public HostInstance[] HostInstances { get; set; }

        [DataMember]
        public Server[] Servers { get; set; }

        [DataMember]
        public Application[] Applications { get; set; }
    }
}