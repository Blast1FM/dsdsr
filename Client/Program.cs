using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client;


class Client
{
    private const string ServerIp = "127.0.0.1";
    private const int Port = 3333;

    static void Main()
    {
        IPAddress ipAddress = IPAddress.Parse(ServerIp);
        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, Port);

        // Создаем сокет и подключаемся к серверу
        Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect(remoteEndPoint);
        Console.WriteLine("Connected to the server.");

        using (NetworkStream stream = new NetworkStream(sender))
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
        {
            while (true)
            {
                Console.WriteLine("Enter operation (add/subtract) and two numbers (e.g., add 1 2):");
                string input = Console.ReadLine();
                if (input.ToLower() == "exit") break;

                try
                {
                    string[] parts = input.Split(' ');
                    string operation = parts[0];
                    int[] values = { int.Parse(parts[1]), int.Parse(parts[2]) };

                    var request = new JsonMessage
                    {
                        Type = "request",
                        Data = new { operation, values },
                        Status = "success"
                    };

                    string jsonRequest = JsonSerializer.Serialize(request);

                    // Отправляем запрос
                    writer.WriteLine(jsonRequest);
                    writer.Flush();

                    // Получаем ответ
                    string jsonResponse = reader.ReadLine();
                    if (jsonResponse == null) break; // Сервер закрыл соединение

                    var response = JsonSerializer.Deserialize<JsonMessage>(jsonResponse);

                    if (response.Type == "response")
                    {
                        Console.WriteLine($"Result: {response.Data}");
                    }
                    else if (response.Type == "error")
                    {
                        Console.WriteLine($"Error: {response.Data}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
        Console.WriteLine("Disconnected from the server.");
    }
}

class JsonMessage
{
    public string Type { get; set; }
    public object Data { get; set; }
    public string Status { get; set; }
}