public sealed class ItemGenerator : IObjectGenerator<Item>
{
    public IEnumerable<Item> Generate(int count)
    {
        var rnd = Random.Shared;
        for (var i = 0; i < count; i++)
        {
            yield return new Item
            {
                Name = "Item" + rnd.Next(1, 1000),
                Price = rnd.Next(10, 500),
                Delivery = (DeliveryMode)rnd.Next(0, 2),
                DeliveryDate = DateTime.Now.AddDays(rnd.Next(1, 30))
            };
        }
    }
}
