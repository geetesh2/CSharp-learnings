namespace Application;

public class Shadowing
{
    public static void Run()
    {
        Parent obj = new Child();

        Console.WriteLine("Method Hiding:");
        obj.Show();

        Console.WriteLine();

        Console.WriteLine("Method Overriding:");
        obj.Print();
    }
}

class Parent
{
    // Normal method
    public void Show()
    {
        Console.WriteLine("Parent Show");
    }

    // Virtual method
    public virtual void Print()
    {
        Console.WriteLine("Parent Print");
    }
}

class Child : Parent
{
    // Hiding / Shadowing
    public new void Show()
    {
        Console.WriteLine("Child Show");
    }

    // Overriding
    public override void Print()
    {
        Console.WriteLine("Child Print");
    }
}