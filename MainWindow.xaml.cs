using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace auto_trade
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
            var jp = JObject.Parse(json);

            foreach (JObject item in jp["result"]) {
                listTest.Items.Add(item["MarketCurrency"]);

            }

            //using (var context = new tradeEntities())
            //{
            //    var ex = context.exchanges;
            //    foreach (var item in ex)
            //    {
            //        listTest.Items.Add(item.name);
            //    }
            //}
        }
    }
}
