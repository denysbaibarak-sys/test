using System.Runtime.Serialization;

[DataContract]
public class Food
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public double Price { get; set; }
}