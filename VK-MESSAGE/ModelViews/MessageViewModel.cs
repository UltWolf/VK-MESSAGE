using System.Linq;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VK_MESSAGE.Services;
using VK_MESSAGE.Services.Abstracts;
using VkNet;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Utils;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;
using System.Security.Cryptography;

namespace VK_MESSAGE.ModelViews
{
    public class MessageViewModel:BaseModelView
    {
        public ICommand GetFriendsCommand { get; set; }
        public ICommand GetMessagesCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public Visibility VisibilityLoading
        {
            get
            {
                return _visibilyLoading;
            }
            set
            {
                _visibilyLoading = value;
                OnPropertyChanged("VisibilityLoading");
            }
        } 
        private readonly static RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private Visibility _visibilyLoading;
        public Visibility VisibilityMessage
        {
            get
            {
                return _visibilityMessage;
            }
            set
            {
               _visibilityMessage = value;
                OnPropertyChanged("VisibilityMessage");
            }
        }
        public User Friend { get => _friend; set {
                _friend = value;
                OnPropertyChanged("Fried");
            } }
        private User _friend;
        private Visibility _visibilityMessage;
        private VkApi api;
        private List<string> proxyList = new List<string>();
        public ObservableCollection<User> Friends { get => _friends; set { _friends = value;
                OnPropertyChanged("Friends");
            } } 
        private ObservableCollection<User> _friends;
        public ObservableCollection<Message> Messages { get => _messages; set { _messages = value;
                OnPropertyChanged("Messages");
    }
}
        private ObservableCollection<Message> _messages;
public string Message { get =>_message; set { this._message = value;
                OnPropertyChanged("Message");
            } }
        private string _message;
        private int index = 0;
        private Models.User user;
        public MessageViewModel(Models.User user)
        {
            this.user = user;
            this.GetFriendsCommand = new Command(GetFriends);
            this.GetMessagesCommand = new Command(GetMessages);
            this.SendMessageCommand = new Command(SendMessage);
            this.Authorize();
        }
        public void Authorize()
        {
            Task.Run(() => Login());
        } 
       
        public void Login()
        {
            this.VisibilityLoading = Visibility.Visible;
            this.VisibilityMessage = Visibility.Hidden;
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create("http://foxtools.ru/Proxy?country=RU&al=True&am=True&ah=True&ahs=True&http=True&https=True");
            httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0";

            var httpResponse = httpRequest.GetResponse();
            string html = "";
            using (Stream stream = httpResponse.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    html = sr.ReadToEnd();
                }
            }
            var parser = new HtmlParser();
            var body = parser.ParseDocument(html);
            var table = body.GetElementById("theProxyList");

            var trs = table.GetElementsByTagName("tr");
            for (int i = 1; i < trs.Length; i++)
            {
                var tds = trs[i].GetElementsByTagName("td");
                string proxy = tds[1].TextContent;
                string port = tds[2].TextContent;
                proxyList.Add(proxy + ":" + port);
            }
            ConnectToVk(proxyList[index]);
            if (index < proxyList.Count)
            {
                Login(proxyList[index]);
            }
        }
        public void ConnectToVk(string httpProxy)
        {
            try
            {
                var request = HttpWebRequest.Create("https://vk.com");
                request.Proxy = new System.Net.WebProxy(httpProxy);
                var httpResponse = request.GetResponse();
                string html = "";
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        html = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                index++;
                if (index < proxyList.Count)
                {

                    ConnectToVk(proxyList[index]);
                }
            }
        }
        public void Login(string proxy)
        {
            try
            {
                string ip = proxy.Split(':')[0];
                int port = int.Parse(proxy.Split(':')[1]);
                IWebProxy httpProxy = new System.Net.WebProxy(ip, port);
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddAudioBypass();
                serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
                serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                serviceCollection.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageProperties = true,
                        CaptureMessageTemplates = true,

                    });
                });
                NLog.LogManager.LoadConfiguration("nlog.config");
                if (user.UseProxy)
                {

                    serviceCollection.AddHttpClient<IRestClient, RestClientWithUserAgent>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { Proxy = httpProxy, UseProxy = true });

                }
                this.api = new VkApi(serviceCollection);
                if (user.UseProxy)
                {
                    api.Browser.Proxy = httpProxy;
                }
                api.Authorize(new ApiAuthParams
                {
                    Login = user.Login,
                    Password = user.Password

                });

                this.Friends =  new ObservableCollection<User>(api.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams
                {
                    UserId = api.UserId,
                    Fields = ProfileFields.All
                }));
                
                    NLog.LogManager.Shutdown();
              
            }
            
            catch (HttpRequestException ex)
            {
                index++;
                if (index < proxyList.Count)
                {
                    Login(proxyList[index]);
                }
                else
                {
                    MessageBox.Show("We have some troubles, please write to us");
                }
            }
            catch (Exception ex) when (ex is AggregateException || ex is SocketException)
            {
                index++;
                if (index < proxyList.Count)
                {
                    Login(proxyList[index]);
                }
                else
                { 
                    MessageBox.Show("We have some troubles, please write to us");
                }

            }
            this.VisibilityLoading = Visibility.Hidden;
            this.VisibilityMessage = Visibility.Visible;
        }
        public void GetFriends(object obj)
        {
            
        }
        public void GetMessages(object obj)
        {
            this.VisibilityLoading = Visibility.Visible;
            this.VisibilityMessage = Visibility.Hidden;
            Task.Run(() =>
            {
                this.Messages = new ObservableCollection<Message>(api.Messages.GetHistory(new MessagesGetHistoryParams() { UserId = Friend.Id }).Messages);
               
                this.VisibilityLoading = Visibility.Hidden;
                this.VisibilityMessage = Visibility.Visible;
            });
          
        }
        public void GetMessages()
        {
            Task.Run(() =>
            {
                this.Messages = new ObservableCollection<Message>(api.Messages.GetHistory(new MessagesGetHistoryParams() { UserId = Friend.Id }).Messages);
            });
        }
        private int GenerateRandomId()
        {
            
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return  BitConverter.ToInt32(bytes, 0);
        }
        public void SendMessage(object obj)
        {
            Task.Run(() =>
            {
                try
                {
                    this.api.Messages.Send(new MessagesSendParams() { Message = Message,  PeerId = Friend.Id, RandomId = GenerateRandomId() });
                    this.Message = "";

                    GetMessages();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Oh, we catch some error, please try again");
                }
            });
        }
    }
}
