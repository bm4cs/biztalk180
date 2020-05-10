using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class HostInstance
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String ExpectedState { get; set; }
    }
}