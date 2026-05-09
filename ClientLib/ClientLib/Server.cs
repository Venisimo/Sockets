using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ClientLib
{
    public class Server
    {
        static void Main()
        {
            // IP-адрес сервера (локальный) и порт
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true) // цикл ожидания новых клиентов
            {
                TcpClient client = listener.AcceptTcpClient();

                var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint != null)
                    Console.WriteLine($"Клиент соединился с адреса {remoteEndPoint.Address}");

                NetworkStream stream = client.GetStream();

                // Передаем список дисков один раз при подключении
                string drives = GetDriveList();
                byte[] drivesData = Encoding.UTF8.GetBytes(drives);
                stream.Write(drivesData, 0, drivesData.Length);

                // Внутренний цикл - обслуживаем одного клиента пока он не отключится
                while (client.Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0) break; // клиент отключился

                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Запрос: {request}");

                        string response = ProcessRequest(request);
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    catch
                    {
                        break; // клиент отключился или ошибка сети
                    }
                }

                client.Close();
                Console.WriteLine("Клиент отключился.");
            }
        }

        public static string GetDriveList()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string result = "";
            foreach (DriveInfo drive in drives)
            {
                result += drive.Name + ";";
            }
            return result;
        }

        public static string ProcessRequest(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    // Это каталог - передаем структуру
                    string[] entries = Directory.GetFileSystemEntries(path);
                    string result = "DIR";
                    foreach (string entry in entries)
                    {
                        result += Path.GetFileName(entry) + ";";
                    }
                    return result;
                }
                else if (File.Exists(path))
                {
                    // Это файл - передаем содержимое
                    string content = File.ReadAllText(path);
                    return $"FILE{path}:\n\n{content}";
                }
                else
                {
                    throw new Exception($"Ошибка: '{path}' не существует или не является файлом/каталогом");
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
        public static TcpListener Start(string serverAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(serverAddress);
            TcpListener listener = new TcpListener(ip, port);
            
            listener.Start();

            return listener;
        }
        public static (TcpClient, string) ConnectClient(TcpListener listener)
        {
            TcpClient client = listener.AcceptTcpClient();

            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint != null)
            {
                return (client, $"Клиент {DateTime.Now} соединился с адреса {remoteEndPoint.Address}");
            }

            throw new Exception("Клиент не смог подключиться");
        }
        public static void WaitingClients(TcpListener listener)
        {
            while (true) // цикл ожидания новых клиентов
            {
                TcpClient client = listener.AcceptTcpClient();

                var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint != null)
                { 
                    Console.WriteLine($"Клиент соединился с адреса {remoteEndPoint.Address}");
                }
                    

                NetworkStream stream = client.GetStream();

                // Передаем список дисков один раз при подключении
                string drives = GetDriveList();
                byte[] drivesData = Encoding.UTF8.GetBytes(drives);
                stream.Write(drivesData, 0, drivesData.Length);

                // Внутренний цикл - обслуживаем одного клиента пока он не отключится
                while (client.Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0) break; // клиент отключился

                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Запрос: {request}");

                        string response = ProcessRequest(request);
                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    catch
                    {
                        break; // клиент отключился или ошибка сети
                    }
                }

                client.Close();
                Console.WriteLine("Клиент отключился.");
            }
        }
        
    }
}
