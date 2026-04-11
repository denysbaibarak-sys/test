using System.Runtime.Serialization;

[DataContract]
public class User
{
    [DataMember]
    public string Login { get; set; }

    [DataMember]
    public string Password { get; set; }
}