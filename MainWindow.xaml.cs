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

using System.Diagnostics;
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

        public void DebugWriteWithTime(string msg) {
            DateTime now = DateTime.Now;
            Debug.WriteLine("[" + String.Format("{0:yyyy-MM-dd h:mm:ss.FFF}", now) +"] " + msg);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DebugWriteWithTime("get json - start");
            var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
            DebugWriteWithTime("get json - end");
            var jp = JObject.Parse(json);

            if ((bool)jp["success"])
            {
                using (var context = new tradeEntities())
                {
                    DebugWriteWithTime("loop - start");
                    foreach (JObject item in jp["result"])
                    {
                        Markets et = new Markets();
                        et.exchange_id = 1; // bittrex
                        et.currency = item["MarketCurrency"].ToString();
                        et.currency_long = item["MarketCurrencyLong"].ToString();
                        et.base_currency = item["BaseCurrency"].ToString();
                        et.min_trade_size = (Decimal)item["MinTradeSize"];
                        et.market_name = item["MarketName"].ToString();
                        et.is_active = (bool)item["IsActive"];
                        et.created_at = (DateTime)item["Created"];
                        context.markets.Add(et);

                        //listTest.Items.Add(item["MarketCurrency"]);
                    }
                    DebugWriteWithTime("loop - end");
                    DebugWriteWithTime("db insert - start");
                    context.SaveChanges();
                    DebugWriteWithTime("db insert - end");
                }
            }
            else
            {

            }

            

            //foreach (JObject item in jp["result"]) {
                

            //    listTest.Items.Add(item["MarketCurrency"]);

            //}

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
