using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClientApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public NetworkStream stream;
        public TcpClient client;
        private string currentDirectory = "";

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ipAddress = ipEntry.Text;
                int port = Convert.ToInt32(portEntry.Text);

                (client, stream) = Client.Connect(ipAddress, port);
                MessageBox.Show("Соединение с сервером установлено!", "Подключение");

                string drivers = Client.GetDriversList(stream);
                AddToComboBox(drivers);

                ClientLog.Text += $"Клиент получил {DateTime.Now} {drivers}\n";

                Button_Connect.IsEnabled = false;
                Button_Disconnect.IsEnabled = true;
                Button_Send.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
            }
        }

        private void AddToComboBox(string drivers)
        {
            string[] driversList = drivers.Split(';');

            DriverList.Items.Clear();

            foreach (var driver in driversList)
            {
                DriverList.Items.Add(driver);
            }
        }

        private void SendAndReceiveData(string selected)
        {
            try
            { 
                Client.SendPathToServer(selected, stream);
                string res = Client.ReceiveResFromServer(stream);

                if (res.StartsWith("FILE"))
                {
                    string subRes = res.Substring(4);

                    ClientLog.Text += $"Клиент получил {DateTime.Now} {subRes}\n";

                    FilesListBox.Visibility = Visibility.Collapsed;
                    FileContentTextBox.Visibility = Visibility.Visible;
                    FileContentTextBox.Text = subRes;
                }
                else
                {
                    string subRes = res.Substring(3);

                    ClientLog.Text += $"Клиент получил {DateTime.Now} {subRes}\n";

                    currentDirectory = selected;
                    string[] files = subRes.Split(';');
                    FileContentTextBox.Visibility = Visibility.Collapsed;
                    FilesListBox.Visibility = Visibility.Visible;
                    FilesListBox.Items.Clear();

                    foreach (string file in files)
                    {
                        if (!string.IsNullOrWhiteSpace(file)) FilesListBox.Items.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка получения");
            }
        }
        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesListBox.SelectedItem == null)
            { 
                return;
            }

            string selected = FilesListBox.SelectedItem.ToString();

            string fullPath = System.IO.Path.Combine(currentDirectory, selected);

            DriverList.Text = fullPath;
        }

        private void FilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesListBox.SelectedItem == null)
            { 
                return;
            }

            string selected = DriverList.Text.ToString();

            currentDirectory = selected;

            SendAndReceiveData(selected);
        }

        private void DriverList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string selected = DriverList.Text;
                if (!string.IsNullOrEmpty(selected))
                {
                    currentDirectory = selected;
                    SendAndReceiveData(selected);
                }
            }
        }

        private void SendToServer_Click(object sender, RoutedEventArgs e)
        {
            string selected = DriverList.Text;
            if (string.IsNullOrEmpty(selected))
            {
                MessageBox.Show("Выберете файл или каталог!", "Ошибка передачи");
                return;
            }

            SendAndReceiveData(selected);
        }

        private void DriverList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriverList.SelectedItem == null)
            { 
                return;
            }

            currentDirectory = DriverList.SelectedItem.ToString();

            DriverList.Text = currentDirectory;

            SendAndReceiveData(currentDirectory);
        }

        private void Disconnect()
        {
            try
            {
                client.Close();
                client = null;
                stream = null;
                Button_Connect.IsEnabled = true;
                Button_Disconnect.IsEnabled = false;
                Button_Send.IsEnabled = false;
                MessageBox.Show($"Соединение прервано", "Отключение");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Отключение");
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Button_Exit(object sender, RoutedEventArgs e)
        {
            client?.Close();
            Application.Current.Shutdown();
        }

        private void ClientLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClientLog.ScrollToEnd();
        }
    }
}
