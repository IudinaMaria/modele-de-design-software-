# Лабораторная работа №4
# Цель работы
Изучить и сравнить различные архитектурные способы представления свойств объекта в программной системе, включая статические (классы, иерархии, интерфейсы, traits) и динамические подходы (object[], компоненты, словари, ECS).
Разработать собственный механизм динамического контекста, позволяющий библиотеке работать с расширяемыми объектами и свойствами, заранее неизвестными потребителю библиотеки.

# 1. Изучение и идентификация динамических контекстов в существующих библиотеках
## 1.1. ASP.NET Core — `HttpContext.Items`
**Ситуация:** Middleware передает данные глубже по конвейеру.
**Тип:** `IDictionary<object, object?>` - динамические свойства.

**Пример:**

```c#
context.Items["RequestStart"] = DateTime.UtcNow;
```

Библиотека ASP.NET не знает, какие данные туда запишет пользователь.

## 1.2. Authentication — `AuthenticationProperties.Items`
Используется для передачи дополнительных параметров между аутентификационными хендлерами.

```c#
authProps.Items["tenant_id"] = "eu-north";
```

## 1.3. MVC Filters — `ActionExecutingContext.ActionParameters`
Параметры действия передаются как словарь `Dictionary<string, object?>`.

## 1.4. FluentValidation — `ValidationContext.RootContextData`
Позволяет валидаторам передавать данные, о которых библиотека не знает.

```c#
context.RootContextData["ip"] = request.IpAddress;
```

## 1.5. HotChocolate GraphQL — `IResolverContext.ContextData`
Позволяет передавать данные между middleware запросов GraphQL.

## 1.6. WPF Behaviors
Используют «attached properties», позволяя динамически добавлять свойства элементам UI, даже если контрол этого свойства не имеет.

**Общий вывод по пункту 1:**
Все эти системы используют динaмические словари или аналогичные механизмы.
Библиотека не определяет структуру данных заранее — только предоставляет механизм хранения.

# 2. Объяснение, почему FatStruct невозможен в библиотеке
**FatStruct** — структура с заранее определённым набором свойств:

```c#
struct FatStruct {
    public int? Age;
    public float? Health;
    public string? CustomData1;
    public object? Metadata;
    // и т.д.
}
```

**Почему это НЕ работает, если проект является библиотекой:**

1. **Библиотека не знает, какие свойства нужны потребителю.**
Она не может заранее включить поля, о которых не знает.
2. **Добавление новых свойств требует перекомпиляции библиотеки.**
Это нарушает изоляцию и независимость.
3. **Conflict-prone: разные проекты потребителя захотят разные свойства.**
Невозможно удовлетворить всех.
4. **Память:** каждый экземпляр сущности содержит ненужные поля.
5. **Структура становится загрязнённой:** нарушается SRP и OCP.

**Главный вывод:**
FatStruct невозможен, потому что библиотека не должна и не может знать, что хотят пользователи.

# 3. Реализация собственной системы динамического контекста

## 3.1. Требования (из задания):
- библиотека должна быть изолирована
- свойства задаются динамически (dictionary или object[])
- проект-потребитель должен иметь возможность добавлять свои ключи
- операции должны работать с данными, заданными потребителем
- нужен централизованный реестр ключей
- ключи должны быть типизированными (generic wrapper)
- при конфликте ключа = ошибка

# 4. Проект 1: Библиотека `DynamicContextLib`
## 4.1. Типизированный ключ — `TypedKey<T>`

```C#
namespace DynamicContextLib;

public sealed class TypedKey<T>
{
    public int Id { get; }

    internal TypedKey(int id)
    {
        Id = id;
    }
}
```

- тип ключа строго определён через `T`
- исключает возможность записать значение другого типа
- позволяет компилятору автоматически проверять корректность использования

## 4.2. Централизованный реестр — `KeyRegistry`

```c#
namespace DynamicContextLib;

public static class KeyRegistry
{
    private static readonly Dictionary<string, int> _registry = new();
    private static int _counter = 0;

    public static TypedKey<T> Register<T>(string name)
    {
        if (_registry.ContainsKey(name))
            throw new InvalidOperationException(
                $"Key '{name}' already registered.");

        var id = _counter++;
        _registry[name] = id;

        return new TypedKey<T>(id);
    }
}
```

**Решает требование:**
- Ключевая строка, реализующая уникальность ключей:

```c#
if (_registry.ContainsKey(name))
    throw new InvalidOperationException($"Key '{name}' already registered.");
```
- Ключевые строки, реализующие типобезопасность:

```c#
public sealed class TypedKey<T>
public void Set<T>(TypedKey<T> key, T value)
public T? Get<T>(TypedKey<T> key)
```

## 4.3. Динамический контекст — `DynamicContext`

```c#
namespace DynamicContextLib;

public sealed class DynamicContext
{
    private readonly Dictionary<int, object> _data = new();

    public void Set<T>(TypedKey<T> key, T value)
    {
        _data[key.Id] = value!;
    }

    public T? Get<T>(TypedKey<T> key)
    {
        return _data.TryGetValue(key.Id, out var obj)
            ? (T)obj
            : default;
    }
}
```

**Решает требование:** динамическое хранение через словарь `private readonly Dictionary<int, object> _data = new();`.

## 4.4. Базовая сущность — `Entity`
```c#
namespace DynamicContextLib;

public class Entity
{
    public DynamicContext Context { get; } = new();
}
```

# 5. Проект 2: Потребитель `GameProject`
## 5.1. Пользовательские ключи — `GameKeys.cs`

```c#
using DynamicContextLib;

public static class GameKeys
{
    public static readonly TypedKey<int> Health =
        KeyRegistry.Register<int>("Game.Health");

    public static readonly TypedKey<string> Owner =
        KeyRegistry.Register<string>("Game.Owner");
}
```

**Это выполняет требование:**
- потребитель может добавить собственные ключи `KeyRegistry.Register<int>("Game.Health");`
- реестр предотвратит конфликт имени
```c#
if (_registry.ContainsKey(name))
    throw new InvalidOperationException();
```
через вызов `KeyRegistry.Register<string>("Game.Owner");`
- ключи типизированы `TypedKey<int>`, `TypedKey<string>`


## 5.2. Операции — `GameSystems.cs`

```c#
using DynamicContextLib;

public static class GameSystems
{
    public static void Damage(Entity e, int amount)
    {
        var hp = e.Context.Get(GameKeys.Health) ?? 0;
        e.Context.Set(GameKeys.Health, Math.Max(0, hp - amount));
    }
}
```

**Выполняет требование:**
- потребитель может задавать свои операции `public static void Damage(Entity e, int amount)`
- операции используют данные, которые библиотека не знала `var hp = e.Context.Get(GameKeys.Health) ?? 0;`

## 5.3. Демонстрация работы — `Program.cs`
**больше анализа по каждому пункту вместо вывода пункта 6**


```c#
using DynamicContextLib;

class Program
{
    static void Main()
    {
        var e = new Entity();

        e.Context.Set(GameKeys.Health, 150);
        e.Context.Set(GameKeys.Owner, "Player1");

        Console.WriteLine(e.Context.Get(GameKeys.Health)); // 150

        GameSystems.Damage(e, 40);

        Console.WriteLine(e.Context.Get(GameKeys.Health)); // 110
    }
}
```

# 6. Ход мыслей при выполнении работы

1. Сначала изучены механизмы динамического контекста в ASP.NET, MVC, FluentValidation, GraphQL, WPF.
Все используют словари для передачи данных, неизвестных библиотеке.

2. Оценены подходы:
- Статические (классы, интерфейсы) — не подходят.
- FatStruct — невозможно использовать в библиотеке.
- object[] — слишком неудобно.
- Dictionary — оптимальное решение.
3. Требование предотвращать конфликты ключей - нужен центральный реестр.
4. Для типизации создан generic-ключ TypedKey<T>.
5. Реализован динамический контекст по аналогии с HttpContext.Items.
6. Потребительский проект был изолирован, но смог добавлять данные и операции.