using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClientLib;

namespace ServerApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener listener;
        TcpClient client;
        bool ServerOn = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Start()
        {
            try
            {
                string ipAddress = ipEntry.Text;
                int port = Convert.ToInt32(portEntry.Text);

                listener = Server.Start(ipAddress, port);

                MessageBox.Show("Сервер включен!");
                ServerOn = true;
                Run_Server.Content = "Отключить сервер";

                ServerLog.Text += $"{DateTime.Now} Сервер запущен на {ipAddress}:{port}\n";
                // Запускаем ожидание клиентов в фоновом потоке
                Task.Run(() => WaitingClients());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска сервера: {ex.Message}");
            }
        }

        private void ServerLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerLog.ScrollToEnd();
        }

        private void WaitingClients()
        {
            while (ServerOn)
            {
                try
                {
                    // Ждём клиента в фоновом потоке — UI не зависает
                    (TcpClient newClient, string message) = Server.ConnectClient(listener);
                    client = newClient;

                    // Обновляем UI через Dispatcher
                    Dispatcher.Invoke(() => ServerLog.Text += message + "\n");

                    NetworkStream stream = client.GetStream();

                    string drives = Server.GetDriveList();
                    byte[] drivesData = Encoding.UTF8.GetBytes(drives);
                    stream.Write(drivesData, 0, drivesData.Length);

                    while (client.Connected)
                    {
                        try
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0) break;

                            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Dispatcher.Invoke(() => ServerLog.Text += $"[{DateTime.Now}] Запрос: {request}\n");

                            string response = Server.ProcessRequest(request);
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseData, 0, responseData.Length);
                        }
                        catch
                        {
                            break;
                        }
                    }

                    client.Close();
                    Dispatcher.Invoke(() => ServerLog.Text += "Клиент отключился.\n");
                }
                catch
                {
                    // listener.Stop() бросает исключение — выходим из цикла
                    break;
                }
            }
        }

        private void Run_Server_Click(object sender, RoutedEventArgs e)
        {
            if (!ServerOn)
            {
                Start();
            }
            else
            {
                if (client != null && client.Connected)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] msg = Encoding.UTF8.GetBytes("SERVER_SHUTDOWN");
                        stream.Write(msg, 0, msg.Length);
                    }
                    catch { }
                }
                ServerOn = false;
                client?.Close();
                listener?.Stop();
                MessageBox.Show("Сервер отключен");
                Run_Server.Content = "Запустить сервер";
            }
        }
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            ServerOn = false;
            listener?.Stop();
            Close();
        }
    }
}
