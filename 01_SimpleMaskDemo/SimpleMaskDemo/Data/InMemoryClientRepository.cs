using System;
using System.Collections.Generic;
using System.Linq; // фильтрация
using SimpleMaskDemo.Domain;

namespace SimpleMaskDemo.Data;

// : обязан реализовать все методы IClientRepository
public sealed class InMemoryClientRepository : IClientRepository
{
    // readonly можно присвоить только один раз (обычный список в памяти)
    private readonly List<Client> _items = new();

    public void Add(Client c) => _items.Add(c);

    public IEnumerable<Client> GetAll() => _items.ToList();

    public IEnumerable<Client> FindByName(string firstName)
        => _items.Where(c => string.Equals(c.FirstName, firstName, StringComparison.OrdinalIgnoreCase));

    private static bool EqualsByMask(Client a, Client b, ClientFields mask)
    {
        // HasFlag проверяет выбран ли бит и сравнивает а и б
        if (mask.HasFlag(ClientFields.Id) && a.Id != b.Id) return false;
        // регистр приравнивает
        if (mask.HasFlag(ClientFields.FirstName) &&
            !string.Equals(a.FirstName, b.FirstName, StringComparison.OrdinalIgnoreCase)) return false;
        if (mask.HasFlag(ClientFields.LastName) && !string.Equals(a.LastName, b.LastName, StringComparison.OrdinalIgnoreCase)) return false;
        // берется разница модулей
        if (mask.HasFlag(ClientFields.Rating) &&
            Math.Abs(a.Rating - b.Rating) > float.Epsilon) return false;
        // SequenceEqual для массивов сравнение поэлементу и по порядку
        if (mask.HasFlag(ClientFields.BelongsToGroups) &&
            !a.BelongsToGroups.SequenceEqual(b.BelongsToGroups)) return false;

        if (mask.HasFlag(ClientFields.IsActive) && a.IsActive != b.IsActive) return false;

        return true;
    }

    public int CopyDataByMask(Client source, ClientFields compareMask, ClientFields copyMask)
    {
        // счетчик показывает сколько изменили дефолт 0
        int updated = 0;

        foreach (var c in _items)
        {
            // ReferenceEquals гаранитя что не будет копирования в себя или себя
            if (ReferenceEquals(c, source)) continue;            // не трогаем сам source
            if (!EqualsByMask(c, source, compareMask)) continue; // фильтруем равных по compareMask

            if (copyMask.HasFlag(ClientFields.Id)) c.Id = source.Id;
            if (copyMask.HasFlag(ClientFields.FirstName)) c.FirstName = source.FirstName;
            if (copyMask.HasFlag(ClientFields.LastName)) c.LastName = source.LastName;
            if (copyMask.HasFlag(ClientFields.Rating)) c.Rating = source.Rating;
            if (copyMask.HasFlag(ClientFields.BelongsToGroups)) c.BelongsToGroups = source.BelongsToGroups.ToArray();
            if (copyMask.HasFlag(ClientFields.IsActive)) c.IsActive = source.IsActive;

            updated++;
        }

        return updated;
    }
}
