using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


class Program
{
    private static Action[] _worksStack; // Тип Action - тип делегата. Делегат - это ссылка на метод.

    private static float[,] a;
    private static float[,] b;
    private static float[,] result;
    private const int arraySize = 4;                                          //*const
    private static int[] conTimer = new int[8];
    private static int[] parTimer = new int[8];

    private static void FillMatrix()
    {
        Random random = new Random();
        a = new float[arraySize, arraySize];
        FillWithRandom(random, a);
        b = new float[arraySize, arraySize];
        FillWithRandom(random, b);
        result = new float[arraySize, arraySize];
    }

    private static void FillWithRandom(Random random, float[,] matrix)
    {
        for (int i = 0; i < arraySize; i++)
        {
            for (int j = 0; j < arraySize; j++)
            {
                matrix[i, j] = (float)random.NextDouble();                    //*NextDouble
            }
        }
    }

    static void Main()
    {
        FillMatrix();
        //Console.WriteLine("Введите количество задач:");
        ////string workAmountString = Console.ReadLine();
        //int workAmount = int.Parse(workAmountString);

        //Console.WriteLine("Как вы хотите выполнить работу? \n введите:"); // символ '\n' - это перенос строки
        //Console.WriteLine("\"par\" для выполнения параллельно \n\"con\" для выполнения последовательно"); // символ '\"' здесь позволяет добавить ковычки в строку
        //string dispatchWay = Console.ReadLine();
        //DispatchWithParametrs(workAmount, dispatchWay);
        //for (int i = 0; i < arraySize; i++)
        //{
        //    for (int j = 0; j < arraySize; j++)
        //    {
        //        Console.WriteLine($"{i}, {j} = {result[i, j]}");
        //    }
        //}
        //Console.WriteLine("Program complete!");
        //Console.ReadLine();

        Console.WriteLine($"Main Thread Id: {Environment.CurrentManagedThreadId}");

        DispatchWithType(DispatchType.consistently);

        DispatchWithType(DispatchType.parallel);

        for (int i = 0; i < conTimer.Length; i++)
        {
            Console.WriteLine($"Time of consistently execution of {i+1} parts : {conTimer[i]}");
        }
        for (int i = 0; i < parTimer.Length; i++)
        {
            Console.WriteLine($"Time of parallel execution of {i + 1} parts : {parTimer[i]}");
        }
    }

    private static void DispatchWithType(DispatchType dispatchWay)
    {
        for (int i = 1; i <= 8; i++)
        {
           
            DispatchWithParametrs(i, dispatchWay);
           
        }
    }

    private static void DispatchWithParametrs(int workAmount, DispatchType dispatchWay)
    {
        ScheduleWork(workAmount);
        switch (dispatchWay) //switch - удобный оператор ветвления, позволяющий не городить громоздкие "if-else"
        {
            case DispatchType.parallel:
                DispatchMultythread();
                break;

            case DispatchType.consistently:
                DispatchInMainThread();
                break;
            default:
                Console.WriteLine("Неизвестная команда!");
                break;
        }
    }

    private static void ScheduleWork(int workAmount)
    {
        _worksStack = new Action[workAmount];                                             //*_workStack
        int length = arraySize * arraySize;
        int elementsInWork = length / workAmount;
        int remains = length - elementsInWork * (workAmount - 1);

        int i;
        for (i = 0; i < workAmount - 1; i++)
        {
            int start = elementsInWork * i;
            int end = elementsInWork * i + elementsInWork;
            _worksStack[i] = () => DoWork(start, end); 
        }
        i = workAmount - 1;
        _worksStack[i] = () => DoWork(elementsInWork * i, elementsInWork * i + remains);

    }

    private static void DispatchInMainThread()                      //последовательное
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < _worksStack.Length; i++)
        {
            
            _worksStack[i].Invoke();
        }
        stopwatch.Stop();
        conTimer[_worksStack.Length-1] = (int)stopwatch.ElapsedMilliseconds;
    }

    private static void DispatchMultythread()                            //параллельное 
    {
        Task[] tasks = new Task[_worksStack.Length];                     //Task
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < _worksStack.Length; i++)
        {
            tasks[i] = new Task(_worksStack[i]);
            tasks[i].Start();
        }
        for (int i = 0; i < _worksStack.Length; i++)
        {
            tasks[i].Wait();
        }
        stopwatch.Stop();
        parTimer[_worksStack.Length-1] = (int)stopwatch.ElapsedMilliseconds;
    }
    private static int Two2One(int i, int j, int size)
    {
        return i * size + j;
    }

    private static (int i, int j) One2Two(int index, int size)
    {
        return (index / size, index % size);
    }

    private static void DoWork(int start, int end)
    {
        Console.WriteLine($"Id: {Environment.CurrentManagedThreadId}"); 
        for (int i = start; i < end; i++)
        {
            (int x, int y) = One2Two(i, arraySize);
            for (int j = 0; j < 1000000; j++)
            {
                result[x, y] = a[x, y] + b[x, y];
            }
            
        }
    }

    public enum DispatchType
    {
        consistently = 0,
        parallel = 1
    }
}
