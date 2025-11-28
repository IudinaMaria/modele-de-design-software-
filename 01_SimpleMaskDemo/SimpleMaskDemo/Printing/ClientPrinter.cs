using System;
using System.Text;
using SimpleMaskDemo.Domain;

namespace SimpleMaskDemo.Printing;

// по факту набор функций, а не объект
public static class ClientPrinter
{
    // Статическая печать по bool-маске: выводим только выбранные поля
    public static void Print(Client c, ClientFieldMaskBool mask)
    {
        // собирает код по кусочкам
        var sb = new StringBuilder();
        // Append добавляет в конец текст. mask - булевое свойство проеряющее на тру/фолс
        if (mask.Id) sb.Append($"Id={c.Id}; ");
        if (mask.FirstName) sb.Append($"FirstName={c.FirstName}; ");
        if (mask.LastName) sb.Append($"LastName={c.LastName}; ");
        if (mask.Rating) sb.Append($"Rating={c.Rating}; ");
        if (mask.BelongsToGroups) sb.Append($"Groups=[{string.Join(',', c.BelongsToGroups)}]; ");
        if (mask.IsActive) sb.Append($"IsActive={c.IsActive}; ");
        Console.WriteLine(sb.ToString().Trim());
    }
}
