using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public class SendPort
	{
		[DataMember]
		public String Name { get; set; }
		[DataMember]
		public ExpectedInstanceState ExpectedState { get; set; }
	}
}