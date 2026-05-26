using System.Text.Json;

namespace Application;

public class Serialization
{
    public static void Run()
    {
        // Create object
        var user = new User
        {
            Id = 1,
            Name = "Geetesh",
            Email = "geetesh@test.com"
        };

        // SERIALIZATION (Object → JSON)
        string json = JsonSerializer.Serialize(
            user,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        Console.WriteLine("Serialized JSON:");
        Console.WriteLine(json);

        Console.WriteLine();

        // DESERIALIZATION (JSON → Object)
        User? deserializedUser =
            JsonSerializer.Deserialize<User>(json);

        Console.WriteLine("Deserialized Object:");
        Console.WriteLine($"Id: {deserializedUser?.Id}");
        Console.WriteLine($"Name: {deserializedUser?.Name}");
        Console.WriteLine($"Email: {deserializedUser?.Email}");
        
        var user2 = new User
        {
            Id = 1,
            Name = "Geetesh"
        };

        // Object → byte[]
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(user2);

        Console.WriteLine("Serialized Bytes:");

        foreach (var b in bytes)
        {
            Console.Write($"{b} ");
        }

        Console.WriteLine();
        Console.WriteLine();

        // byte[] → Object
        User? result =
            JsonSerializer.Deserialize<User>(bytes);

        Console.WriteLine("Deserialized Object:");

        Console.WriteLine(result?.Id);
        Console.WriteLine(result?.Name);
    }
}   

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";
}