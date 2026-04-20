using System.Runtime.Serialization;

[DataContract]
public class User
{
    [DataMember]
    public string Token { get; set; }

    [DataMember]
    public int Id { get; set; }
    [DataMember]
    public string Login { get; set; }

    [DataMember]
    public string Password { get; set; }

    [DataMember]
    public string Email { get; set; }

    [DataMember]
    public string Phone { get; set; }

    [DataMember]
    public string RegistrationDate { get; set; }

    [DataMember]
    public string Address { get; set; }
}