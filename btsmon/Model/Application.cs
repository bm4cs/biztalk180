using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public class Application
	{
		[DataMember]
		public String Name { get; set; }
		[DataMember]
		public SendPort[] SendPorts { get; set; }
		[DataMember]
		public SendPortGroup[] SendPortGroups { get; set; }
		[DataMember]
		public ReceiveLocation[] ReceiveLocations { get; set; }
		[DataMember]
		public Orchestration[] Orchestrations { get; set; }
	}
}