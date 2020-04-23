
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace btsmon.Model
{
    [DataContract]
    public class Configuration
    {
        [DataMember]
        public Environment[] Environments { get; set; }

        public static Configuration LoadLocalFile(string fileName)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Configuration));
            Stream fileStream = File.OpenRead(Path.Combine(System.Environment.CurrentDirectory, fileName));
            return serializer.ReadObject(fileStream) as Configuration; 
        }
    }
}
