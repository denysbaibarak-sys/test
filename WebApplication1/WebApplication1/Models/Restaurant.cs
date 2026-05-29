using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

[DataContract]
public class Restaurant
{
    [Key]
    [DataMember]
    public int Id { get; set; }
    
    [DataMember]
    public int OwnerId { get; set; }
    
    [DataMember]
    public string OwnerLogin { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Address { get; set; }

    [DataMember]
    public double Rating { get; set; }

    [DataMember]
    public string DeliveryTime { get; set; }

    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string Distance { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public string ImagePath { get; set; }

    [DataMember]
    public List<Food> Menu { get; set; }
}