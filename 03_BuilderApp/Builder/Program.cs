#pragma warning disable CS8321 // фигня для компилятора, чтобы не ругался из-за функций

// билдер, внутри мутабл модель которая умеет строить встроенные сущности
var clinic = new ClinicBuilder();

// билдер клиники создает билдер палаты (RoomBuilder)
var room = clinic.Room()
    .Number("A-101")
    .ConstantBloosPressureMeasurement(120, 80); // чтобы от обраалось всегда. типа устанавливает выше номер палаты и кладет внутрь прибор давления

// высокоуровневый конфигуратор (общие настройки для будущих пациентов) по умолчанию комната и возраст
var s = clinic.Scope()
    .Room(room)
    .Age(32); // fluent

{
    // создаю пациента через скоре. на него накладывается все из скоре и локально задается имя и возраст (перезапишется)
    var p = s.Patient()
        .Age(18)
        .Name("Ana");
}
{
    // так же пациент, но по дефолту возраст из скоре
    var p = s.Patient()
        .Name("Ion");
}
{
    // так же пациент, но по дефолту возраст из скоре
    var p = s.Patient()
        .Name("Maria");
}

// конвертация mutable в immutable с валидацией. то есть вернется Clinic с массивами Patients[] и Rooms[] (их уже нельзя менять)
var immClinic = clinic.Build();
Console.WriteLine(immClinic.Patients[0].Age);
Console.WriteLine(immClinic.Patients[1].Age);
Console.WriteLine(immClinic.Patients[2].Age);
var measurement = immClinic.Patients[0].Room?.Device?.Measure(); // добавила и тогда будет как цепочка пациент-палата-прибор и один из давлений
if (measurement.HasValue)
{
    var (systolic, diastolic) = measurement.Value;
    Console.WriteLine($"{systolic}/{diastolic}");
}
// подавляет предупреждение переменная не используется
_ = immClinic;
// чтобы дальше (альтернативная инициализация и классы) не выполнялись при запуске этого фрагмента как скрипта
return;

// ручная сборка immutable объектов
static void InitializationWithoutBuilder()
{
    var room = new Room
    {
        Number = "A-101",
    };
    var ana = new Patient
    {
        Age = 18,
        Name = "Ana",
        Room = room,
    };
    var clinic = new Clinic
    {
        Patients = [ana],
        Rooms = [room],
    };
    _ = clinic;
}

// интерфейс или имплементация
interface IBloodPressureDevice // интерфейс прибора, типа любой прибор палаты должен уметь измерять давление
{
    (int systolic, int diastolic) Measure();
}

sealed class SimpleBpDevice : IBloodPressureDevice // просто хранит всегда два числа и дает результатт давления
{
    private readonly int _systolic;
    private readonly int _diastolic;

    // тупой тип устройства может возвращать только одно значение
    public SimpleBpDevice(int systolic, int diastolic)
    {
        _systolic = systolic;
        _diastolic = diastolic;
    }

    public (int systolic, int diastolic) Measure()
    {
        return (_systolic, _diastolic);
    }
}

// ип устройства более продвинутый
sealed class AdvancedDevice : IBloodPressureDevice
{
    public (int systolic, int diastolic) Measure()
    {
        return (130, 85);
    }
}

// чертеж прибора 
sealed class MutableBloodPressureDeviceConfig
{
    public bool IaXonarnr; // если тру то константный
    public int A; // пармаетры давления
    public int B;
}

// мутабле модели - изменяемые классы
sealed class MutableClinic
{
    // контейнер для данных, запрет на наследование
    public List<MutablePatient> Patients = new();
    public List<MutableRoom> Rooms = new();
}

// открытые поля, некорректные значения, если возраст не указан -1
sealed class MutablePatient
{
    public const int InvalidAge = -1;

    public int Age = InvalidAge;
    public string? Name = null;
    public MutableRoom? Room = null;
}

// пока не задали номер будет нулл
sealed class MutableRoom
{
    public string? Number;
    public IBloodPressureDevice? Device;
    // не создает сразу прибор, а сохраняет конфигурацию в мутабл, а уже после в буилд создает прибор
    public MutableBloodPressureDeviceConfig? BloodPressureDevice;
    public bool HasAdvancedBloodPressureMeasurement;
}

// билдер клиники
sealed class ClinicBuilder
{
    public MutableClinic Model = new();

    // создает пустого mutable-пациента добавляет его в клинику и возвращает PatientBuilder для конфигурации
    public PatientBuilder Patient()
    {
        var m = new MutablePatient();
        Model.Patients.Add(m);
        return new(m);
    }

    // так же для палаты
    public RoomBuilder Room()
    {
        var m = new MutableRoom();
        Model.Rooms.Add(m);
        return new(m);
    }

    // валидация и конвертация Mutable в Immutable. готовый массив Immutable палат
    public Clinic Build()
    {
        var retRooms = new Room[Model.Rooms.Count];
        //прохидт по всем палатам, если нет номера не проходит валидацию
        for (int i = 0; i < retRooms.Length; i++)
        {
            var mut = Model.Rooms[i];
            if (mut.Number is not { } number)
            {
                throw new InvalidOperationException(
                    // тобы проще искать, где в коде создавали объект GetCreationInfo
                    $"Validation error: room number not specified (created at {GetCreationInfo()})");
            }

            // тут создается прибор при билдере, типа мини паттерн
            IBloodPressureDevice? device = null;
            if (mut.BloodPressureDevice is { IaXonarnr: true })
            {
                device = new SimpleBpDevice(mut.BloodPressureDevice.A, mut.BloodPressureDevice.B);
            }
            else if (mut.HasAdvancedBloodPressureMeasurement)
            {
                device = new AdvancedDevice();
            }
            // прошедл проверку и создает Immutable палату и кладет ее в массив
            var immut = new Room
            {
                Number = number,
                Device = device,
            };
            retRooms[i] = immut;
        }

        // коллекция дикционари с ключ значение. по факту ссылка ма палату мутабл при сборке находим иммутабл палату
        var mutToImmutRoom = new Dictionary<MutableRoom, Room>(retRooms.Length);
        for (int i = 0; i < retRooms.Length; i++)
        {
            var mut = Model.Rooms[i];
            var immut = retRooms[i];
            mutToImmutRoom.Add(mut, immut);
        }

        //итоговый массив пациентов, вытаскиваем его палту из словаря и даем 
        var retPatients = new Patient[Model.Patients.Count];
        for (int i = 0; i < retPatients.Length; i++)
        {
            var mut = Model.Patients[i];
            var room = mut.Room != null ? mutToImmutRoom[mut.Room] : null;

            // валидация пациента, имя обяз, возраст нужен, иначе throw
            if (mut.Name is not { } name)
            {
                throw new InvalidOperationException(
                    $"Validation error: patient name not given (created at {GetCreationInfo()})");
            }
            if (mut.Age == MutablePatient.InvalidAge)
            {
                throw new InvalidOperationException(
                    $"Validation error: patient age not given (created at {GetCreationInfo()})");
            }

            // сборка пациента и суем в массив
            var immut = new Patient
            {
                Age = mut.Age,
                Name = name,
                Room = room,
            };
            retPatients[i] = immut;
        }

        // возвращает нашу клинику
        return new Clinic
        {
            Patients = retPatients,
            Rooms = retRooms,
        };
    }

    // вытаскиваем одну строку из stack trace (используется для откладки) и видим где именно в коде ошибка 
    private static string GetCreationInfo()
    {
        var trace = Environment.StackTrace.Split('\n');
        return trace.Length > 3 ? trace[3].Trim() : "unknown location";
    }

    //точка входа на высокоуровневую конфигурацю. то есть тут и запускаем билдер
    public ClinicScopeBuilder Scope()
    {
        return new(this);
    }
}

// по факту хранит тут ссылку на сновной билдер, а так же на шаблон куда записываются значения
sealed class ClinicScopeBuilder
{
    public readonly ClinicBuilder ClinicBuilder;
    public readonly PatientBuilder PatientBuilder;

    // создаем нового пустогоо и тут задаем настройки
    public ClinicScopeBuilder(ClinicBuilder clinic)
    {
        ClinicBuilder = clinic;
        var patient = new MutablePatient();
        PatientBuilder = new(patient);
    }

    // создает новгоо пациента через главный билдер и копирует в него поля из скоре.
    public PatientBuilder Patient()
    {
        var p = ClinicBuilder.Patient();
        if (PatientBuilder.Model.Age != MutablePatient.InvalidAge)
        {
            p.Model.Age = PatientBuilder.Model.Age;
        }
        if (PatientBuilder.Model.Name is { } name)
        {
            p.Model.Name = name;
        }
        if (PatientBuilder.Model.Room is { } room)
        {
            p.Model.Room = room;
        }
        return p;
    }

    // создаем палату внутри скоре
    public RoomBuilder Room()
    {
        return ClinicBuilder.Room();
    }

    // fluent методы которые позволяют менять внутри скоре шаблон и возвращают this 
    public ClinicScopeBuilder Age(int age)
    {
        PatientBuilder.Age(age);
        return this;
    }
    public ClinicScopeBuilder Name(string name)
    {
        PatientBuilder.Name(name);
        return this;
    }
    public ClinicScopeBuilder Room(RoomBuilder room)
    {
        PatientBuilder.Room(room);
        return this;
    }
}

// ссылка на конкретный MutablePatient
sealed class PatientBuilder
{
    public readonly MutablePatient Model;
    public PatientBuilder(MutablePatient model)
    {
        Model = model;
    }

    // сеттер возраста с базовой валидацией, те не может быть отриц
    public PatientBuilder Age(int age)
    {
        if (age < 0)
        {
            throw new InvalidOperationException(
                $"Invalid age specified (created at {Environment.StackTrace.Split('\n')[2].Trim()})");
        }
        Model.Age = age;
        return this; // fluent
    }

    // сеттер для имени и палаты
    public PatientBuilder Name(string name)
    {
        Model.Name = name;
        return this; // fluent
    }

    public PatientBuilder Room(RoomBuilder room)
    {
        Model.Room = room.Room;
        return this; // fluent
    }
}

// билдер палаты щадает ее номер
sealed class RoomBuilder
{
    public readonly MutableRoom Room;

    public RoomBuilder(MutableRoom room)
    {
        Room = room;
    }

    public RoomBuilder Number(string number)
    {
        Room.Number = number;
        return this; // fluent
    }

    // тут мы говорим, что хотим простой прибор
    public RoomBuilder ConstantBloosPressureMeasurement(int a, int b)
    {
        if (Room.BloodPressureDevice == null)
        {
            Room.BloodPressureDevice = new MutableBloodPressureDeviceConfig();
        }

        Room.BloodPressureDevice.IaXonarnr = true;
        Room.BloodPressureDevice.A = a;
        Room.BloodPressureDevice.B = b;
        Room.HasAdvancedBloodPressureMeasurement = false;
        return this;
    }

    // тут наоборот реализуется продвинутый прибор
    public RoomBuilder AdvancdBloodPressureMeasurement()
    {
        Room.BloodPressureDevice = null;
        Room.HasAdvancedBloodPressureMeasurement = true;
        return this;
    }
}

// массив пациентов и палат immutable подход. required и init делают поля доступными только при создании
sealed class Clinic
{
    public required Patient[] Patients { get; init; }
    public required Room[] Rooms { get; init; }
}

#pragma warning disable CS0649

// так же для пациента и клиники
sealed class Patient
{
    public required string Name;
    public required int Age;
    public Room? Room;
}

sealed class Room
{
    public required string Number;
    public IBloodPressureDevice? Device;
}
#pragma warning restore CS0649
