using System.Runtime.Serialization;

namespace btsmon.Model
{
    [DataContract]
    public class BtsMonSendPort
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ExpectedState { get; set; }
    }
}