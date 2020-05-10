using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public enum ExpectedInstanceState
	{
		[EnumMember]
		DontCare,
		[EnumMember]
		Unenlisted,
		[EnumMember]
		Started
	}
}