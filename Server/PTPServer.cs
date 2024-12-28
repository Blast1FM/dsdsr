using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server;

public class PTPServer
{
    private const int Port = 3333;
    private DateOnly _cachedDate;
    RandomIntCacheManager _CachedInt = new();

    public void Run()
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

    private void HandleClient(Socket clientSocket)
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
                        var request = JsonSerializer.Deserialize<PTPMessage>(jsonRequest);
                        var response = ProcessRequest(request);
                        string jsonResponse = JsonSerializer.Serialize(response);

                        writer.WriteLine(jsonResponse);
                        writer.Flush(); // Убедимся, что данные отправлены
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = new PTPMessage
                        {
                            Type = "reponse",
                            Data = ex.Message,
                            Status = "error"
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

    private PTPMessage ProcessRequest(PTPMessage request)
    {
        if (request.Type != "request")
        {
            throw new Exception("Invalid message type");
        }

        string operation = request.Data;

        PTPMessage resultMessage = operation switch
        {
            "getRandomNumber" => HandleGetRandomNumberRequest(request),
            "getDate" => HandleGetDateRequest(request), 
            _ => new PTPMessage{Type = "response",Data = "",Status = "error"}
        };

        return resultMessage;
    }

    private PTPMessage HandleGetDateRequest(PTPMessage request)
    {
        if(DateOnly.FromDateTime(request.CreatedAt) != _cachedDate)
        {
            _cachedDate = DateOnly.FromDateTime(DateTime.Now);
        }

        var responseMessage = new PTPMessage
        {
            Type = "response",
            Data = _cachedDate.ToString(),
            Status = "success"
        };

        return responseMessage;
        
    }

    private PTPMessage HandleGetRandomNumberRequest(PTPMessage request)
    {
        int randomNumber = _CachedInt.RetrieveRandomNumber(request.CreatedAt);

        var responseMessage = new PTPMessage
        {
            Type = "response",
            Data = randomNumber.ToString(),
            Status = "success"
        };

        return responseMessage;
    }
}
