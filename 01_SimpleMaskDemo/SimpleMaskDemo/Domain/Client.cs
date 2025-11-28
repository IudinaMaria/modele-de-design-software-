namespace SimpleMaskDemo.Domain;

// sealed не наслед класс, как бы фиксирует, что только так будет модель выглядеть
public sealed class Client
{
    public int Id { get; set; }
    //required по факту заставляет чтобы этобыло заполнено
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public float Rating { get; set; }
    // по факту групп - енум (перечисление) если к 2 группам относится и тд
    // как с++ гет прочитать сет присвоить значения
    // System.Array.Empty<Group>() пустой массив, чтобы не нулл или будет ошибка
    public Group[] BelongsToGroups { get; set; } = System.Array.Empty<Group>();

    public bool IsActive { get; set; }
    // override изменяет стандарт поедеия без него бы вывел просто SimpleMaskDemo.Domain.Client с ним норм вывод так же может сравнить 
    public override string ToString()
        => $"Id={Id}; FirstName={FirstName}; LastName={LastName}; Rating={Rating}; Groups=[{string.Join(',', BelongsToGroups)}]; IsActive={IsActive}";
}
