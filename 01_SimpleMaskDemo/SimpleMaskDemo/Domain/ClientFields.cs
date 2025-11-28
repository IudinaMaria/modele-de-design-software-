namespace SimpleMaskDemo.Domain;

// говорит о том, что флаги можно комбинировать по битам
[System.Flags]
public enum ClientFields
{
    // ничего не выбрано маска флагов
    None = 0,
    // каждый флагзанимает свой бит каждый сдвиг идет влево в двоисном виде, таким сдвигом есть гаранитя уникальности, а значит лече миксовать |, &, ~
    // 000001
    Id = 1 << 0,
    // 000010
    FirstName = 1 << 1,
    // 000100
    LastName = 1 << 2,
    // 001000
    Rating = 1 << 3,
    // 010000
    BelongsToGroups = 1 << 4,
    // 100000
    IsActive = 1 << 5,

    // OR объеденение маски выборка всех
    All = Id | FirstName | LastName | Rating | BelongsToGroups | IsActive
}
