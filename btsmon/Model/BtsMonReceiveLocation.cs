using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class BtsMonReceiveLocation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ExpectedState { get; set; }
    }
}