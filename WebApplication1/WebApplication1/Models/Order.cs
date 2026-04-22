using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class Order
{
    [DataMember]
    public string OrderId { get; set; }

    [DataMember]
    public int UserId { get; set; }
    [DataMember]
    public int RestaurantId { get; set; }

    [DataMember]
    public string RestaurantName { get; set; }

    [DataMember]
    public string OrderDate { get; set; }

    [DataMember]
    public DateTime UpdatedAt { get; set; }

    [DataMember]
    public List<Food> OrderedItems { get; set; }

    [DataMember]
    public decimal TotalPrice { get; set; }

    [DataMember]
    public string Status { get; set; }

    [DataMember]
    public string DeliveryAddress { get; set; }
}