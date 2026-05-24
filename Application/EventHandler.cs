class Order
{
    public event EventHandler OnCreated;

    public void Create()
    {
        Console.WriteLine("Order created");

        if (OnCreated != null)
        {
            OnCreated(this, EventArgs.Empty);
        }
    }
}


class Email
{
    public static void Send(object sender, EventArgs e)
    {
        Console.WriteLine($"Send an email");
    }
}

class SMS
{
    public static void Send(object sender, EventArgs e)
    {
        Console.WriteLine($"Send an SMS");
    }
}


class Program
{
    static void Main(string[] args)
    {
        var order = new Order();

        order.OnCreated += Email.Send;
        order.OnCreated += SMS.Send;

        order.Create();
    }
}