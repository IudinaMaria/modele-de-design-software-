using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

// по факту папка для имен, чтобы не было конфликта классов
namespace PipelineNS
{
    // интерфейс. сообщаю любому контексту, чтобы был стоп в выполнении шага
    public interface IHasIsDone
    {
        bool IsDone { get; set; }
    }

    // дженерик, который может работать с разными типами данных. ексекьют делает, интоспект описывает
    public interface IPipelineStep<TContext>
    {
        void Execute(TContext context);
        void Introspect(StringBuilder sb);
    }

    // Chain of Responsibility позволяет передавать запросы последовательно по цепочке обработчиков
    // сходу задаю, что каждый конктекс должен уметь выполнять IHasIsDone
    public class Pipeline<TContext> where TContext : IHasIsDone
    {
        public List<IPipelineStep<TContext>> Steps { get; } = new();

        // каждый шаг перебираем, если IsDone тру - выкидывает брейк и останавливаем цепочку. это и есть Responsibility Chain
        public void Execute(TContext context)
        {
            foreach (var step in Steps)
            {
                if (context.IsDone) break;
                step.Execute(context);
            }
        }

        // если находит первый по типу шаг, то записывает его в newStep, точнее заменяте элемент списка на это
        public void ReplaceFirstInstance(Type typeToReplace, IPipelineStep<TContext> newStep)
        {
            var idx = Steps.FindIndex(s => s.GetType() == typeToReplace);
            if (idx >= 0)
                Steps[idx] = newStep;
        }

        // точно так же, но заменяет все совпадения
        public void ReplaceAll(Type typeToReplace, IPipelineStep<TContext> newStep)
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                if (Steps[i].GetType() == typeToReplace)
                    Steps[i] = newStep;
            }
        }

        // оборачивает все шаги, на входе старый шаг, на выходе новый, но внутри все то же
        public void WrapAll(Func<IPipelineStep<TContext>, IPipelineStep<TContext>> wrapFunc)
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i] = wrapFunc(Steps[i]);
            }
        }

        // находит первый шаг указанного типа, если нет, то выходит. удаляет с текущей позиции и вставляет на позицию индекс
        public void MoveTo(Type typeToMove, int index)
        {
            var el = Steps.FirstOrDefault(s => s.GetType() == typeToMove);
            if (el == null) return;
            Steps.Remove(el);
            if (index < 0) index = 0;
            if (index > Steps.Count) index = Steps.Count;
            Steps.Insert(index, el);
        }

        // Интроспекция возможность анализировать и получать информацию о собственных объектах во время выполнения
        // создается StringBuilder (удобнее чем обычный стринг, который неизменяемый) и каждый шаг дописать своё описание
        public void PrintAllSteps()
        {
            var sb = new StringBuilder();
            foreach (var s in Steps)
            {
                s.Introspect(sb);
            }
            Console.WriteLine(sb.ToString());
        }
    }

    // делаем пациента
    public class PatientContext : IHasIsDone
    {
        public bool IsDone { get; set; }
        public string Name { get; set; } = "";
        public int NIHSS { get; set; }
        public string Diagnosis { get; set; } = "";
    }

    // шаги пациента сохраняет имя в NewName, записывает в контекст, пишет свой тип и параметр 
    public class ChangeNameStep : IPipelineStep<PatientContext>
    {
        public string NewName { get; set; } = "";

        public void Execute(PatientContext context)
        {
            context.Name = NewName;
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(ChangeNameStep)} (NewName=\"{NewName}\")");
        }
    }

    // так же, но с NIHS выполняет
    public class SetNIHSStep : IPipelineStep<PatientContext>
    {
        public int Score { get; set; }

        public void Execute(PatientContext context)
        {
            context.NIHSS = Score;
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(SetNIHSStep)} (Score={Score})");
        }
    }
    // так же с диагнозом

    public class SetDiagnosisStep : IPipelineStep<PatientContext>
    {
        public string Dx { get; set; } = "";

        public void Execute(PatientContext context)
        {
            context.Diagnosis = Dx;
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(SetDiagnosisStep)} (Dx=\"{Dx}\")");
        }
    }

    // печатет состояние контекста по факту
    public class PrintPatientStep : IPipelineStep<PatientContext>
    {
        public void Execute(PatientContext context)
        {
            Console.WriteLine($"Patient: Name={context.Name}, NIHSS={context.NIHSS}, Dx={context.Diagnosis}, IsDone={context.IsDone}");
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(PrintPatientStep)}");
        }
    }

    // шаг стоп, если NIHSS ≤ порога, то будет тру и дальше не выполнятся шаги
    public class StopIfMildNIHSStep : IPipelineStep<PatientContext>
    {
        public int Threshold { get; set; } = 5;

        public void Execute(PatientContext context)
        {
            if (context.NIHSS <= Threshold)
            {
                context.IsDone = true; // Chain of Responsibility: останавливаем
            }
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(StopIfMildNIHSStep)} (Threshold={Threshold})");
        }
    }

    // второй контекст 

    public class LabOrderContext : IHasIsDone
    {
        public bool IsDone { get; set; }
        public string PatientName { get; set; } = "";
        public List<string> Tests { get; set; } = new();
        public bool Urgent { get; set; }
    }

    // добавляет тест, если имя не пустое
    public class AddTestStep : IPipelineStep<LabOrderContext>
    {
        public string TestName { get; set; } = "";

        public void Execute(LabOrderContext context)
        {
            if (!string.IsNullOrWhiteSpace(TestName))
                context.Tests.Add(TestName);
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(AddTestStep)} (TestName=\"{TestName}\")");
        }
    }

    // срочность помечает
    public class MarkUrgentStep : IPipelineStep<LabOrderContext>
    {
        public bool Value { get; set; }

        public void Execute(LabOrderContext context)
        {
            context.Urgent = Value;
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(MarkUrgentStep)} (Value={Value})");
        }
    }

    // печатает состояние
    public class PrintLabOrderStep : IPipelineStep<LabOrderContext>
    {
        public void Execute(LabOrderContext context)
        {
            Console.WriteLine($"LabOrder: Patient={context.PatientName}, Urgent={context.Urgent}, Tests=[{string.Join(", ", context.Tests)}], IsDone={context.IsDone}");
        }

        public void Introspect(StringBuilder sb)
        {
            sb.AppendLine($"Step: {nameof(PrintLabOrderStep)}");
        }
    }

    public static class Functions
    {
        // демонстрирую пайплайн для пациента, по факту вспомогательный статический класс
        // создает как деом для пациента в Pipeline<PatientContext>
        public static void PatientPipelineDemo()
        {
            var pipeline = new Pipeline<PatientContext>();

            pipeline.Steps.Add(new ChangeNameStep { NewName = "Luke" });
            pipeline.Steps.Add(new SetNIHSStep { Score = 3 });
            pipeline.Steps.Add(new StopIfMildNIHSStep { Threshold = 5 });
            pipeline.Steps.Add(new SetDiagnosisStep { Dx = "Minor ischemic stroke" });
            pipeline.Steps.Add(new PrintPatientStep());

            Console.WriteLine("== Introspect Patient pipeline ==");
            pipeline.PrintAllSteps();

            Console.WriteLine("== Execute Patient pipeline ==");
            pipeline.Execute(new PatientContext());

            // высокоуровневых методов
            pipeline.ReplaceFirstInstance(typeof(SetDiagnosisStep), new SetDiagnosisStep { Dx = "TIA suspected" });
            pipeline.MoveTo(typeof(PrintPatientStep), 0);

            Console.WriteLine("== Introspect after modifications ==");
            pipeline.PrintAllSteps();

            Console.WriteLine("== Execute after modifications ==");
            pipeline.Execute(new PatientContext());
        }

        // демонстрация пайплайна для лабораторного заказа
        public static void LabOrderPipelineDemo()
        {
            var pipeline = new Pipeline<LabOrderContext>();

            pipeline.Steps.Add(new AddTestStep { TestName = "Complete Blood Count" });
            pipeline.Steps.Add(new AddTestStep { TestName = "D-dimer" });
            pipeline.Steps.Add(new MarkUrgentStep { Value = true });
            pipeline.Steps.Add(new PrintLabOrderStep());

            Console.WriteLine("== Introspect LabOrder pipeline ==");
            pipeline.PrintAllSteps();

            Console.WriteLine("== Execute LabOrder pipeline ==");
            pipeline.Execute(new LabOrderContext
            {
                PatientName = "Lucy"
            });

            // Замена всех AddTestStep на один новый шаг
            pipeline.ReplaceAll(typeof(AddTestStep), new AddTestStep { TestName = "CRP" });

            Console.WriteLine("== Introspect LabOrder after ReplaceAll ==");
            pipeline.PrintAllSteps();

            Console.WriteLine("== Execute LabOrder after ReplaceAll ==");
            pipeline.Execute(new LabOrderContext
            {
                PatientName = "Lucy"
            });
        }
    }

    class Program
    {
        static void Main()
        {

            while (true)
            {
                Console.Write("Enter demo (Patient/Lab/Exit): ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "Patient":
                        {
                            Functions.PatientPipelineDemo();
                            break;
                        }
                    case "Lab":
                        {
                            Functions.LabOrderPipelineDemo();
                            break;
                        }
                    default:
                        {
                            return;
                        }
                }
            }
        }
    }
}
