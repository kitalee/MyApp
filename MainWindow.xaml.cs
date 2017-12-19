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
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;


namespace auto_trade
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        private BackgroundWorker work = new BackgroundWorker();
        

        public MainWindow()
        {
            InitializeComponent();

            work.DoWork += Work_DoWork;
            work.RunWorkerCompleted += Work_RunWorkerCompleted;
            work.WorkerReportsProgress = true;
            work.WorkerSupportsCancellation = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
        }

        public void DebugWriteWithTime(string msg) {
            DateTime now = DateTime.Now;
            Debug.WriteLine("[" + String.Format("{0:yyyy-MM-dd h:mm:ss.FFF}", now) +"] " + msg);
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {

            initDB();




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

        public void initDB() {
            SqlConnection con = null;
            try
            {
                listLog.Items.Clear();
                TradeEntities db = new TradeEntities();

                //if (db.Database.Exists()) {
                //    db.Database.Delete();
                //}
                //db.Database.CreateIfNotExists();

                ////Database.SetInitializer(new CreateDatabaseIfNotExists<TradeEntities>());

                //// Exchanges
                //Exchanges ex = new Exchanges();
                //ex.name = "bittrex";
                //ex.key = "xxxx";
                //db.exchanges.Add(ex);
                //db.SaveChanges();

                //// Markets
                //DebugWriteWithTime("get json - start");
                //var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
                //DebugWriteWithTime("get json - end");
                //var jp = JObject.Parse(json);

                //if ((bool)jp["success"])
                //{
                //    DebugWriteWithTime("loop - start");
                //    foreach (JObject item in jp["result"])
                //    {
                //        Markets et = new Markets();
                //        et.exchange_id = 1; // bittrex
                //        et.currency = item["MarketCurrency"].ToString();
                //        et.currency_long = item["MarketCurrencyLong"].ToString();
                //        et.base_currency = item["BaseCurrency"].ToString();
                //        et.min_trade_size = (Decimal)item["MinTradeSize"];
                //        et.market_name = item["MarketName"].ToString();
                //        et.is_active = (bool)item["IsActive"];
                //        et.created_at = (DateTime)item["Created"];
                //        db.markets.Add(et);

                //        //listTest.Items.Add(item["MarketCurrency"]);
                //    }
                //    DebugWriteWithTime("loop - end");
                //    DebugWriteWithTime("db insert - start");
                //    db.SaveChanges();
                //    DebugWriteWithTime("db insert - end");

                //}
                //else
                //{
                //    throw new System.Exception("Fail to get data from exchanges");
                //}

                // Coin Tables

                var marketNames = from m in db.markets
                                  where m.market_name.Contains("BTC-") && m.is_active == true
                                  select m;

                string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyCon2"].ConnectionString;
                con = new SqlConnection(conStr);
                con.Open();



                foreach (var item in marketNames)
                {
                    var aaa = item.market_name;
                    listLog.Items.Add(aaa);

                    string sql = "";
                    sql += "CREATE TABLE [dbo].[" + item.market_name + "] ( ";
                    sql += "    [id] int IDENTITY(1,1) NOT NULL, ";
                    sql += "    [market_id] int  NULL, ";
                    sql += "    [bid] decimal(18,8)  NULL, ";
                    sql += "    [ask] decimal(18,8)  NULL, ";
                    sql += "    [last] decimal(18,8)  NULL, ";
                    sql += "    [avg] decimal(18,8)  NULL, ";
                    sql += "    [acc_rate] int  NULL, ";
                    sql += "    [created_at] datetime  NULL ";
                    sql += "); ";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {

                listLog.Items.Add(ex.Message);
            }
            finally {
                if (con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
            }
            




        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            listLog.Items.Clear();
            listLog.Items.Add(string.Format("{0}{1}", DateTime.Now, Environment.NewLine));
            listLog.Items.Add(string.Format("Button Clicked then disabled{0}", Environment.NewLine));
            this.btnTest.IsEnabled = false; //#1

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(()=> {
                System.Threading.Thread.Sleep(2000); //#2
                this.btnTest.IsEnabled = true; //#3
                listLog.Items.Add(string.Format("Button pressed then enabled{0}{0}", Environment.NewLine));
            }));


            //System.Threading.Thread.Sleep(2000); //#2

            //this.btnTest.IsEnabled = true; //#3
            //listLog.Items.Add(string.Format("Button pressed then enabled{0}{0}", Environment.NewLine));
        }

        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            
            work.RunWorkerAsync();

        }

        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            listLog.Items.Add("complete!");
            btnMonitor.IsEnabled = true;
        }

        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            btnMonitor.IsEnabled = false;
            //throw new NotImplementedException();
            for (int i = 1; i < 1000; i++) {
                listLog.Items.Add(i.ToString());
            }
        }
    }
}
