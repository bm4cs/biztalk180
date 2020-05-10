using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public class Orchestration
	{
		[DataMember]
		public String Name { get; set; }
		[DataMember]
		public ExpectedInstanceState ExpectedState { get; set; }
	}
}