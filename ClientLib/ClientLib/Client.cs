using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

public class Client
{
    static void Main()
    {
        Console.Write("Введите адрес сервера (IP или localhost): ");
        string serverAddress = Console.ReadLine();
        int port = 8888;

        try
        {
            // Подключаемся к серверу
            TcpClient client = new TcpClient(serverAddress, port);
            NetworkStream stream = client.GetStream();

            // Принимаем список дисков от сервера
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string drivesList = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(drivesList);

            // Запрашиваем путь у пользователя
            Console.Write("\nВведите путь к каталогу или текстовому файлу: ");
            string userPath = Console.ReadLine();

            // Отправляем путь на сервер
            byte[] requestData = Encoding.UTF8.GetBytes(userPath);
            stream.Write(requestData, 0, requestData.Length);

            // Принимаем ответ от сервера
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("\nОтвет сервера:");
            Console.WriteLine(response);

            // Закрываем соединение
            client.Close();
            Console.WriteLine("Соединение закрыто.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    public static (TcpClient, NetworkStream) Connect(string serverAddress, int port)
    {
        TcpClient client = new TcpClient(serverAddress, port);
        NetworkStream stream = client.GetStream();

        return (client, stream);
    }
    public static string GetDriversList(NetworkStream stream)
    { 
        byte[] buffer = new byte[4096];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string drivesList = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        return drivesList;
    }
    public static void SendPathToServer(string path, Stream stream)
    {
        byte[] requestData = Encoding.UTF8.GetBytes(path);
        stream.Write(requestData, 0, requestData.Length);
    }
    public static string ReceiveResFromServer(Stream stream)
    {
        byte[] buffer = new byte[4096];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (response == "SERVER_SHUTDOWN") throw new Exception("Сервер отключился");

        return response;
    }
}