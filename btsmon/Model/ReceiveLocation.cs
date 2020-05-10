using System;
using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public class ReceiveLocation
	{
		[DataMember]
		public String Name { get; set; }
		[DataMember]
		public ExpectedEnableState ExpectedState { get; set; }
	}
}