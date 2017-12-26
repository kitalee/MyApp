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
using System.Data;


namespace auto_trade
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker wInitDB = new BackgroundWorker();
        private BackgroundWorker wMonitor = new BackgroundWorker();
        private static bool isMonitor = false;
        private static DataTable dtMarkets;
        private Dictionary<string, decimal> dic = new Dictionary<string, decimal>();

        public MainWindow()
        {
            InitializeComponent();

            // initdb
            wInitDB.DoWork += WInitDB_DoWork;
            wInitDB.RunWorkerCompleted += WInitDB_RunWorkerCompleted;
            wInitDB.ProgressChanged += WInitDB_ProgressChanged;
            wInitDB.WorkerReportsProgress = true;

            // monitor
            wMonitor.DoWork += wMonitor_DoWork;
            wMonitor.RunWorkerCompleted += wMonitor_RunWorkerCompleted;
            wMonitor.ProgressChanged += WMonitor_ProgressChanged;
            wMonitor.WorkerReportsProgress = true;
            wMonitor.WorkerSupportsCancellation = true;
        }

        private void WInitDB_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void WMonitor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void WInitDB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnInit.IsEnabled = true;
            //throw new NotImplementedException();
        }

        private void WInitDB_DoWork(object sender, DoWorkEventArgs e)
        {
            initDB();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
        }

        private string GetTime() {
            DateTime now = DateTime.Now;
            return "[" + String.Format("{0:yyyy-MM-dd h:mm:ss.FFF}", now) + "] ";
        }

        private void Logging(string msg) {
            DateTime now = DateTime.Now;
            //Debug.WriteLine(GetTime() + msg);

            if (listLog.Dispatcher.CheckAccess())
            {
                listLog.Items.Insert(0, GetTime() + msg);
            }
            else {
                Action updateListLog = () => listLog.Items.Insert(0, GetTime() + msg);
                listLog.Dispatcher.Invoke(updateListLog);
            }
            
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            //initDB();

            btnInit.IsEnabled = false;
            wInitDB.RunWorkerAsync();





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
                Logging("DB Init Start!");
                wInitDB.ReportProgress(0);
                //listLog.Items.Clear();
                TradeEntities db = new TradeEntities();

                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.CreateIfNotExists();

                //Database.SetInitializer(new CreateDatabaseIfNotExists<TradeEntities>());

                // Exchanges
                Exchanges ex = new Exchanges();
                ex.name = "bittrex";
                ex.key = "xxxx";
                db.exchanges.Add(ex);
                db.SaveChanges();

                // Markets
                Logging("get json - start");
                var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getmarkets");
                Logging("get json - end");
                var jp = JObject.Parse(json);

                wInitDB.ReportProgress(10);

                if ((bool)jp["success"])
                {
                    Logging("loop - start");
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
                        db.markets.Add(et);

                        //listTest.Items.Add(item["MarketCurrency"]);
                    }
                    Logging("loop - end");
                    Logging("db insert - start");
                    db.SaveChanges();
                    Logging("db insert - end");

                }
                else
                {
                    throw new System.Exception("Fail to get data from exchanges");
                }

                wInitDB.ReportProgress(70);

                // Coin Tables

                var marketNames = from m in db.markets
                                  where m.market_name.Contains("BTC-") && m.is_active == true
                                  select m;

                string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyCon2"].ConnectionString;
                con = new SqlConnection(conStr);
                con.Open();

                // Create Index
                // markets
                SqlCommand cmd = new SqlCommand("create nonclustered index IDX_MARKET on markets(market_name)", con);
                cmd.ExecuteNonQuery();

                foreach (var item in marketNames)
                {
                    var name = item.market_name;
                    Logging("making table : " + name);

                    string sql = "";
                    sql += "CREATE TABLE [dbo].[" + item.market_name + "] ( ";
                    sql += "    [id] int IDENTITY(1,1) NOT NULL, ";
                    sql += "    [market_id] int  NULL, ";
                    sql += "    [bid] decimal(18,8)  NULL, ";
                    sql += "    [ask] decimal(18,8)  NULL, ";
                    sql += "    [last] decimal(18,8)  NULL, ";
                    sql += "    [avg] decimal(18,8)  NULL, ";
                    sql += "    [acc_rate] int  NULL, ";
                    sql += "    [acc_number] int  NULL, ";
                    sql += "    [created_at] datetime  NULL ";
                    sql += "); ";
                    cmd = new SqlCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    // INDEX
                    cmd = new SqlCommand("create nonclustered index IDX_"+ item.market_name.Replace("-", "_") + " on ["+ item.market_name + "](created_at)", con);
                    cmd.ExecuteNonQuery();
                }

                Logging("DB Init Complete!");
                wInitDB.ReportProgress(100);
            }
            catch (Exception ex)
            {

                Logging(ex.Message);
            }
            finally {
                if (con != null && con.State == System.Data.ConnectionState.Open) {
                    con.Close();
                }
                
            }
            

        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logging("getting data - start");
                var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getticker?market=BTC-ETH");
                var jp = JObject.Parse(json);
                Logging("getting data - end");
                if ((bool)jp["success"])
                {

                    Logging(jp["result"]["Bid"].ToString());
                    Logging(jp["result"]["Ask"].ToString());
                    Logging(jp["result"]["Last"].ToString());
                    //foreach (JProperty item in jp["result"])
                    //{
                    //    listLog.Items.Add("Bid : " + item["Bid"]);
                    //    listLog.Items.Add("Bid : " + item["Ask"]);
                    //    listLog.Items.Add("Bid : " + item["Last"]);
                    //}


                }


                //listLog.Items.Clear();
                //listLog.Items.Add(string.Format("{0}{1}", DateTime.Now, Environment.NewLine));
                //listLog.Items.Add(string.Format("Button Clicked then disabled{0}", Environment.NewLine));
                //this.btnTest.IsEnabled = false; //#1

                //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(()=> {
                //    System.Threading.Thread.Sleep(2000); //#2
                //    this.btnTest.IsEnabled = true; //#3
                //    listLog.Items.Add(string.Format("Button pressed then enabled{0}{0}", Environment.NewLine));
                //}));
            }
            catch (Exception ex)
            {

                Logging(ex.Message);
            }
            

        }

        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection con = null;
            try
            {
                if ((string)btnMonitor.Content == "Start Monitor")
                {
                    listLog.Items.Clear();
                    btnMonitor.Content = "Stop Monitor";
                    isMonitor = true;
                    Logging("===================== Monitoring Start =====================");

                    //dtMarkets
                    //select market_name from markets where is_active = 1 and market_name like 'BTC-%'
                    string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyCon2"].ConnectionString;
                    con = new SqlConnection(conStr);
                    con.Open();
                    string sql = "select market_name from markets where id between 1 and 3";
                    //string sql = "select market_name from markets where is_active = 1 and market_name like 'BTC-%'";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    dtMarkets = new DataTable();
                    dtMarkets.Load(cmd.ExecuteReader());

                    //foreach (DataRow dr in dtMarkets.Rows) {
                    //    Logging(dr["market_name"].ToString());
                    //}

                    wMonitor.RunWorkerAsync(dtMarkets);
                }
                else
                {
                    btnMonitor.Content = "Start Monitor";
                    isMonitor = false;
                }
            }
            catch (Exception ex)
            {
                Logging(ex.Message);
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }



        }

        private void wMonitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (isMonitor) {
                wMonitor.RunWorkerAsync(dtMarkets);
            }
            else
            {
                Logging("===================== Monitoring End =====================");
            }
        }

        private void wMonitor_DoWork(object sender, DoWorkEventArgs e)
        {
            SqlConnection con = null;
            try
            {
                DataTable markets = (DataTable)e.Argument;
                string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyCon2"].ConnectionString;
                con = new SqlConnection(conStr);
                con.Open();

                Logging("getting - START");
                var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getticker?market=BTC-ETH");
                var jp = JObject.Parse(json);
                Logging("getting - END");
                if ((bool)jp["success"])
                {

                    if (dic.ContainsKey("BTC-ETH-LAST"))
                    {

                    }
                    else {

                    }

                    Logging(jp["result"]["Bid"].ToString());
                    Logging(jp["result"]["Ask"].ToString());
                    Logging(jp["result"]["Last"].ToString());
                    //foreach (JProperty item in jp["result"])
                    //{
                    //    listLog.Items.Add("Bid : " + item["Bid"]);
                    //    listLog.Items.Add("Bid : " + item["Ask"]);
                    //    listLog.Items.Add("Bid : " + item["Last"]);
                    //}


                    string sql = "";
                    sql += "insert into [dbo].[BTC-ETH] (market_id, bid, ask, last, created_at) ";
                    sql += "values (1, " + jp["result"]["Bid"].ToString() + ", " + jp["result"]["Ask"].ToString() + ", " + jp["result"]["Last"].ToString() + ", getdate()) ";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.ExecuteNonQuery();

                    Thread.Sleep(500);
                }
                else
                {
                    Logging("FAIL!");
                }






                //foreach (DataRow dr in markets.Rows)
                //{
                //    string name = dr["market_name"].ToString();

                //    Logging("getting " + name + " - START");
                //    var json = new WebClient().DownloadString("https://bittrex.com/api/v1.1/public/getticker?market=" + name);
                //    var jp = JObject.Parse(json);
                //    Logging("getting " + name + " - END");
                //    if ((bool)jp["success"])
                //    {
                //        if (!jp["result"].HasValues)
                //            continue;

                //        Logging(jp["result"]["Bid"].ToString());
                //        Logging(jp["result"]["Ask"].ToString());
                //        Logging(jp["result"]["Last"].ToString());
                //        //foreach (JProperty item in jp["result"])
                //        //{
                //        //    listLog.Items.Add("Bid : " + item["Bid"]);
                //        //    listLog.Items.Add("Bid : " + item["Ask"]);
                //        //    listLog.Items.Add("Bid : " + item["Last"]);
                //        //}

                //        string sql = "";
                //        sql += "insert into [dbo].[" + name + "] (market_id, bid, ask, last, created_at) ";
                //        sql += "values (1, " + jp["result"]["Bid"].ToString() + ", " + jp["result"]["Ask"].ToString() + ", " + jp["result"]["Last"].ToString() + ", getdate()) ";
                //        SqlCommand cmd = new SqlCommand(sql, con);
                //        cmd.ExecuteNonQuery();
                //    }
                //    else
                //    {
                //        Logging("FAIL : " + name);
                //    }
                //}


                //for (int i = 1; i < 100; i++)
                //{
                //    Logging(i.ToString());
                //}

                //Thread.Sleep(5000);
                //throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                Logging(ex.Message);
            }
            finally
            {
                if (con != null && con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}
