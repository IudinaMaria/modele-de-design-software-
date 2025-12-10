public enum DeliveryMode
{
    Pickup,
    Driver,
}

public sealed class Item
{
    public required int Price { get; set; }
    public required string Name { get; set; }
    public required DeliveryMode Delivery { get; set; }
    public required DateTime DeliveryDate { get; set; }

    public override string ToString()
        => $"Item(Name={Name}, Price={Price}, Delivery={Delivery}, Date={DeliveryDate})";
}
