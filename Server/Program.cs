using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server;

class Server
{
    private const int Port = 12345;

    static void Main()
    {
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(localEndPoint);
        listener.Listen(10);

        Console.WriteLine("Server started...");

        while (true)
        {
            Console.WriteLine("Waiting for a connection...");
            Socket clientSocket = listener.Accept();
            Console.WriteLine("Client connected.");

            // Создаем новый поток для обработки клиента
            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }

    private static void HandleClient(Socket clientSocket)
    {
        using (NetworkStream stream = new NetworkStream(clientSocket))
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
        {
            try
            {
                while (true)
                {
                    string jsonRequest = reader.ReadLine();
                    if (jsonRequest == null) break; // Клиент закрыл соединение

                    Console.WriteLine($"Received: {jsonRequest}");

                    try
                    {
                        var request = JsonSerializer.Deserialize<JsonMessage>(jsonRequest);
                        var response = ProcessRequest(request);
                        string jsonResponse = JsonSerializer.Serialize(response);

                        writer.WriteLine(jsonResponse);
                        writer.Flush(); // Убедимся, что данные отправлены
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = new JsonMessage
                        {
                            Type = "error",
                            Data = new { message = ex.Message },
                            Status = "failure"
                        };
                        string jsonErrorResponse = JsonSerializer.Serialize(errorResponse);

                        writer.WriteLine(jsonErrorResponse);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Console.WriteLine("Client disconnected.");
            }
        }
    }

    private static JsonMessage ProcessRequest(JsonMessage request)
    {
        if (request.Type != "request")
        {
            throw new Exception("Invalid message type");
        }

        dynamic data = request.Data;
        string operation = data.operation;
        int[] values = data.values.ToObject<int[]>();

        int result = operation switch
        {
            "add" => values[0] + values[1],
            "subtract" => values[0] - values[1],
            _ => throw new Exception("Unsupported operation")
        };

        return new JsonMessage
        {
            Type = "response",
            Data = new { result },
            Status = "success"
        };
    }
}

class JsonMessage
{
    public string Type { get; set; }
    public object Data { get; set; }
    public string Status { get; set; }
}