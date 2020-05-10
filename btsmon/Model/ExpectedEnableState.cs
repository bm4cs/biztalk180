using System.Runtime.Serialization;

namespace btsmon.Model
{
	[DataContract]
	public enum ExpectedEnableState
	{
		[EnumMember]
		DontCare,
		[EnumMember]
		Disabled,
		[EnumMember]
		Enabled
	}
}