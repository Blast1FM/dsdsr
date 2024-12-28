namespace Server;

class Program
{
    public static void Main()
    {  
        PTPServer server = new();
        server.Run();
    }
}

