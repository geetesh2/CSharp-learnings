class DelegatesAndAnonymusFunctions
{
    delegate int Transfomer(int x);
    static int Square(int x)
    {
        Console.WriteLine("Square called");
        return x * x;
    }
    static int Cube(int x)
    {
        return x * x * x;
    }
    public static void Main(string[] args)
    {
        Transfomer transform = Square;
        transform += Cube;

        Transfomer transform2 = delegate (int x)
        {
           return x * x;
        };

        Console.WriteLine(transform(10)); // 100
    }
}