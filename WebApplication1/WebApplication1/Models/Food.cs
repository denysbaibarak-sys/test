using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

[DataContract]
public class Food
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public decimal Price { get; set; }

    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string ImagePath { get; set; }

    [DataMember]
    public int Quantity { get; set; }
}