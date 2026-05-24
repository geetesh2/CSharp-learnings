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
    public static void main(string[] args)
    {
        Transfomer transform = Square;
        transform += Cube;

        Transfomer transform2 = delegate (int x)
        {
           return x * x;
        };

        Transfomer transfrom3 = (int x) => x * x; // (x) => x * x; // type inference

        Console.WriteLine(transfrom3(10)); // 100
    }
}