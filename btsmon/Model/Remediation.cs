using System;

namespace btsmon.Model
{
    /// <summary>
    ///     Represents an artifact that was not in the expected state.
    /// </summary>
    public class Remediation
    {
        public string Name { get; set; }

        public string ExpectedState { get; set; }

        public string ActualState { get; set; }

        public ArtifactType Type { get; set; }

        public DateTime RepairedTime { get; set; }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}