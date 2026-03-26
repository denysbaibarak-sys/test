using System.Runtime.Serialization;
using System.Collections.Generic;

[DataContract]
public class Restaurant
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Address { get; set; }

    [DataMember]
    public List<Food> Menu { get; set; }
}