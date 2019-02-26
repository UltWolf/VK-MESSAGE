using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using VkNet.AudioBypassService.Extensions;
using System.Net;
using System.Net.Http; 
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;  
using VkNet.Abstractions.Utils;
using VkNet.Enums.Filters; 
using VkNet.Model;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;
using VkNet.Utils;
using VkNet;
using VkNet.AudioBypassService.Utils;
using System.Net.Sockets;
using VK_MESSAGE.ModelViews;

namespace VK_MESSAGE.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingView.xaml
    /// </summary>
    public partial class LoadingView : Window
    {
        public LoadingView(Models.User user)
        {
            InitializeComponent();
            this.DataContext = new MessageViewModel(user);
            Storyboard sb = (Storyboard)this.LoadingIcon.FindResource("spin");
            sb.Begin();
            sb.SetSpeedRatio(27);
        }
          

      
    }
}
