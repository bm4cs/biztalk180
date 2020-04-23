using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class Server
    {
        [DataMember]
        public String Name { get; set; }
    }
}