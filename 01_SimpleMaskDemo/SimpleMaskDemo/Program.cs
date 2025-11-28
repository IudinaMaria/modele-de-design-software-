using System;
using SimpleMaskDemo.Domain;
using SimpleMaskDemo.Data;
using SimpleMaskDemo.Printing;

class Program
{
    static void Main()
    {
        // создается абстракция бд переменная типа интерфейса IClientRepository, а реализация - InMemoryClientRepository
        // repo — переменная, которая хранит ссылку на коллекциою ((добавление, поиск, выборка и т. д.).
        // лучше, чем список, особенно, если потом в норм бд кинуть и просто заменить переменную
        IClientRepository repo = new InMemoryClientRepository();

        // вар сам при компиляции определяет тип переменной, которую я ему присвою
        // создается объект клинта
        var c1 = new Client
        {
            Id = 1,
            FirstName = "Tatiana",
            LastName = "Anastasia",
            // даю понять, что это не дабл, а флоат
            Rating = 4.8f,
            BelongsToGroups = new[] { Group.HighlyValued },
            IsActive = true
        };
        var c2 = new Client
        {
            Id = 2,
            FirstName = "Tatiana",
            LastName = "Anastasia",
            Rating = 3.9f,
            BelongsToGroups = new[] { Group.NotImportant },
            IsActive = false
        };
        var c3 = new Client
        {
            Id = 3,
            FirstName = "Maria",
            LastName = "Iudina",
            Rating = 4.5f,
            BelongsToGroups = new[] { Group.HighlyValued, Group.NotImportant },
            IsActive = true
        };

        // добавляем в нашу абстрактнюу бд
        repo.Add(c1);
        repo.Add(c2);
        repo.Add(c3);

        // foreach перебирает все элементы коллекции по очередно, чище код нежели просто фор
        Console.WriteLine("== Все клиенты ==");
        foreach (var c in repo.GetAll())
            Console.WriteLine(c);

        // поиск
        Console.WriteLine("\n== FindByName(\"Tatiana\") ==");
        foreach (var c in repo.FindByName("Tatiana"))
            Console.WriteLine(c);

        
        // маска, которой определяем ключевые поля для вывода
        Console.WriteLine("\n== Печать по маске (FirstName + LastName + Rating) ==");
        var mask = new ClientFieldMaskBool
        {
            FirstName = true,
            LastName = true,
            Rating = true,
        };
        foreach (var c in repo.GetAll())
            ClientPrinter.Print(c, mask);

        // одноразмерные маски по одному флагу
        Console.WriteLine("\n== Битовые маски: Union / Intersect / Except ==");
        var onlyName = ClientFields.FirstName;
        var onlyGroup = ClientFields.BelongsToGroups;

        var u = ClientFieldMasks.Union(onlyName, onlyGroup);      // Name + Groups
        var i = ClientFieldMasks.Intersect(u, ClientFields.All);  // (остаётся то же)
        var e = ClientFieldMasks.Except(u, onlyGroup);            // оставим только Name

        Console.WriteLine($"Union:     {u}");
        Console.WriteLine($"Intersect: {i}");
        Console.WriteLine($"Except:    {e}");

        Console.WriteLine("\n== До CopyDataByMask ==");
        foreach (var c in repo.GetAll()) Console.WriteLine(c);

        c1.Rating = 5.0f;
        c1.IsActive = true;
        c1.BelongsToGroups = new[] { Group.HighlyValued };


        var compareMask = ClientFields.FirstName | ClientFields.LastName;
        var copyMask = ClientFields.Rating | ClientFields.IsActive | ClientFields.BelongsToGroups;

        int changed = repo.CopyDataByMask(c1, compareMask, copyMask);
        Console.WriteLine($"\nОбновлено объектов: {changed}");

        Console.WriteLine("\n== После CopyDataByMask ==");
        foreach (var c in repo.GetAll()) Console.WriteLine(c);

        Console.WriteLine("\nГотово. Нажми Enter, чтобы выйти.");
        Console.ReadLine();
    }
};