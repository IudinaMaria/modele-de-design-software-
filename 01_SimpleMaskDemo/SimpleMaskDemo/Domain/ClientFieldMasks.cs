namespace SimpleMaskDemo.Domain;

public static class ClientFieldMasks
{
    // Union склеивает наборы
    // | включает всё, что включено хотя бы в одной маске
    public static ClientFields Union(ClientFields a, ClientFields b) => a | b;
    // & оставляет только те биты, которые стоят и в a, и в b
    public static ClientFields Intersect(ClientFields a, ClientFields b) => a & b;
    // & ~ из a вычесть все биты, которые есть в b
    public static ClientFields Except(ClientFields a, ClientFields b) => a & ~b;
}
