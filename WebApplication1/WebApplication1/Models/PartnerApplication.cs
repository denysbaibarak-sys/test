using System;
using System.Runtime.Serialization;

[DataContract]
public class PartnerApplication
{
    [DataMember]
    public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
    [DataMember]
    public int UserId { get; set; }
    [DataMember]
    public string FullName { get; set; }
    [DataMember]
    public string Phone { get; set; }
    [DataMember]
    public string Email { get; set; }
    [DataMember]
    public string Description { get; set; }
    [DataMember]
    public string Status { get; set; } = "Pending";
    [DataMember]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}