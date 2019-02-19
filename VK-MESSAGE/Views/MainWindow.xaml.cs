using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

using VkNet;
using VkNet.Abstractions.Utils;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

namespace VK_MESSAGE
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BinaryFormatter bf = new BinaryFormatter();

        List<string> proxylist = new List<string>();

        int index = 0;

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("user.data"))
            {

                RecoveryUser();
            }
        }

        private void RecoveryUser()
        {
            Models.User user = new Models.User();
            using (FileStream fs = new FileStream("user.data", FileMode.Open))
            {
                try
                {
                    user = (Models.User)bf.Deserialize(fs);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Recovery user has been failed");
                }
            }
            Email.Text = user.Login;
            Password.Text = user.Password;
            UseProxy.IsChecked = user.UseProxy;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            using (FileStream fs = new FileStream("user.data", FileMode.Create))
            {
                bf.Serialize(fs, new Models.User() { Login = Email.Text, Password = Password.Text,UseProxy = (bool)UseProxy.IsChecked });
            }

        }
    }
     
}
