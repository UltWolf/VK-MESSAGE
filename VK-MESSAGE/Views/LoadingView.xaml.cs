using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
 
using VkNet;
using VkNet.Abstractions.Utils;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

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
            this.user = user;
            Login( );
        }
        private List<string> proxyList = new List<string>();
        private int index = 0;
        private Models.User user;
        public void Login( )
        {
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
            Task.Run(() =>
            {
                ConnectToVk(proxyList[index]);
                Login(proxyList[index]);
            });
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
                ConnectToVk(proxyList[index]);
            }
        }
        public void Login(string proxy)
        {
            try
            {
                string ip = proxy.Split(':')[0];
                int port = int.Parse(proxy.Split(':')[1]);


                ServiceCollection serviceCollection = new ServiceCollection();
                if (user.UseProxy)
                {
                    IWebProxy httpProxy = new System.Net.WebProxy(ip, port);
                    serviceCollection.AddHttpClient<IRestClient, RestClient>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { Proxy = httpProxy, UseProxy = true });
                }
                var api = new VkApi(serviceCollection);
                api.Authorize(new ApiAuthParams
                {
                    ApplicationId = 6864097,
                    Login = this.user.Login,
                    Password =  this.user.Password,
                    Settings = Settings.Messages,
                });
                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                {
                    ChatId = api.UserId.Value,
                    Message = "message"
                });
            }
            catch (AggregateException ex)
            {
                index++;
                if (index < proxyList.Count)
                {
                    Login(proxyList[index]);
                }
                MessageBox.Show("We have some troubles, please write to us");
            }
        }
    }
}
