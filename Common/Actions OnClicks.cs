// Actions OnClick
// Part of Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2009 - 2011 Miroslav Popov - All rights reserved!
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using MT4Bridge;

namespace Forex_Strategy_Trader
{
    /// <summary>
    /// Class Actions : Controls
    /// </summary>
    public partial class Actions : Controls, IDisposable 
    {
       

        bool IsRun = true;
        ~Actions()
        {
            IsRun = false;
            if (td != null) td.Abort();
            Dispose(false);
        }
        /// <summary>
        /// Opens the averaging parameters dialog.
        /// </summary>
        protected override void PnlAveraging_Click(object sender, EventArgs e)
        {
            EditStrategyProperties();

            return;
        }

        /// <summary>
        /// Opens the indicator parameters dialog.
        /// </summary>
        protected override void PnlSlot_MouseUp(object sender, MouseEventArgs e)
        {
            Panel panel = (Panel)sender;
            int iTag = (int)panel.Tag;
            if (e.Button == MouseButtons.Left)
                EditSlot(iTag);

            return;
        }

        /// <summary>
        /// Strategy panel menu items clicked
        /// </summary>
        protected override void SlotContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            int iTag = (int)mi.Tag;
            switch (mi.Name)
            {
                case "Edit":
                    EditSlot(iTag);
                    break;
                case "Upwards":
                    MoveSlotUpwards(iTag);
                    break;
                case "Downwards":
                    MoveSlotDownwards(iTag);
                    break;
                case "Duplicate":
                    DuplicateSlot(iTag);
                    break;
                case "Delete":
                    RemoveSlot(iTag);
                    break;
                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// MenuChangeTabs_OnClick
        /// </summary>
        protected override void MenuChangeTabs_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            if (mi.Checked)
                return;

            int iTag = (int)mi.Tag;
            ChangeTabPage(iTag);
            
            return;
        }

        /// <summary>
        /// Performs actions after the button add open filter was clicked.
        /// </summary>
        protected override void BtnAddOpenFilter_Click(object sender, EventArgs e)
        {
            AddOpenFilter();

            return;
        }

        /// <summary>
        /// Performs actions after the button add close filter was clicked.
        /// </summary>
        protected override void BtnAddCloseFilter_Click(object sender, EventArgs e)
        {
            AddCloseFilter();

            return;
        }

        /// <summary>
        /// Remove the corresponding indicator slot.
        /// </summary>
        protected override void BtnRemoveSlot_Click(object sender, EventArgs e)
        {
            int iSlot = (int)((Button)sender).Tag;

            RemoveSlot(iSlot);

            return;
        }

        /// <summary>
        /// Load a color scheme.
        /// </summary>
        protected override void MenuLoadColor_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            if (!mi.Checked)
            {
                Configs.ColorScheme = mi.Name;
            }
            foreach (ToolStripMenuItem tsmi in mi.Owner.Items)
            {
                tsmi.Checked = false;
            }
            mi.Checked = true;

            LoadColorScheme();

            return;
        }

        /// <summary>
        /// Gradient View Changed
        /// </summary>
        protected override void MenuGradientView_OnClick(object sender, EventArgs e)
        {
            Configs.GradientView = ((ToolStripMenuItem)sender).Checked;
            pnlWorkspace.Invalidate(true);
            SetColors();
            return;
        }


        /// <summary>
        /// Strategy IO
        /// </summary>
        protected override void BtnStrategyIO_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;

            switch (btn.Name)
            {
                case "New":
                    NewStrategy();
                    break;
                case "Open":
                    ShowOpenFileDialog();
                    break;
                case "Save":
                    SaveStrategy();
                    break;
                case "SaveAs":
                    SaveAsStrategy();
                    break;
                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// Loads the default strategy.
        /// </summary>
        protected override void MenuStrategyNew_OnClick(object sender, EventArgs e)
        {
            NewStrategy();

            return;
        }

        /// <summary>
        /// Opens the dialog form OpenFileDialog.
        /// </summary>
        protected override void MenuFileOpen_OnClick(object sender, EventArgs e)
        {
            ShowOpenFileDialog();

            return;
        }

        /// <summary>
        /// Saves the strategy.
        /// </summary>
        protected override void MenuFileSave_OnClick(object sender, EventArgs e)
        {
            SaveStrategy();

            return;
        }

        /// <summary>
        /// Opens the dialog form SaveFileDialog.
        /// </summary>
        protected override void MenuFileSaveAs_OnClick(object sender, EventArgs e)
        {
            SaveAsStrategy();

            return;
        }

        /// <summary>
        /// Undoes the strategy.
        /// </summary>
        protected override void MenuStrategyUndo_OnClick(object sender, EventArgs e)
        {
            UndoStrategy();

            return;
        }

        /// <summary>
        /// Copies the strategy to clipboard.
        /// </summary>
        protected override void MenuStrategyCopy_OnClick(object sender, EventArgs e)
        {
            Strategy_XML strategyXML = new Strategy_XML();
            System.Xml.XmlDocument xmlDoc = strategyXML.CreateStrategyXmlDoc(Data.Strategy);
            Clipboard.SetText(xmlDoc.InnerXml);

            return;
        }

        /// <summary>
        /// Pastes a strategy from clipboard.
        /// </summary>
        protected override void MenuStrategyPaste_OnClick(object sender, EventArgs e)
        {
            DialogResult dialogResult = WhetherSaveChangedStrategy();

            if (dialogResult == DialogResult.Yes)
                SaveStrategy();
            else if (dialogResult == DialogResult.Cancel)
                return;

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            Strategy_XML strategyXML = new Strategy_XML();
            Strategy tempStrategy;

            try
            {
                xmlDoc.InnerXml = Clipboard.GetText();
                tempStrategy = strategyXML.ParseXmlStrategy(xmlDoc);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            OnStrategyChange();

            Data.Strategy = tempStrategy;
            Data.StrategyName = tempStrategy.StrategyName;
            Data.Strategy.StrategyName = tempStrategy.StrategyName;

            Data.SetStrategyIndicators();
            RebuildStrategyLayout();
            SetSrategyOverview();

            SetFormText();
            Data.IsStrategyChanged = false;
            Data.LoadedSavedStrategy = Data.StrategyPath;
            Data.StackStrategy.Clear();

            CalculateStrategy(true);
            AfterStrategyOpening();

            return;
        }

        /// <summary>
        /// Loads a dropped strategy.
        /// </summary>
        protected override void LoadDroppedStrategy(string filePath)
        {
            Data.StrategyDir = System.IO.Path.GetDirectoryName(filePath);
            LoadStrategyFile(filePath);
        }

        /// <summary>
        /// Opens the strategy settings dialogue.
        /// </summary>
        protected override void MenuStrategyAUPBV_OnClick(object sender, EventArgs e)
        {
            UsePreviousBarValue_Change();

            return;
        }

        /// <summary>
        /// Export the strategy in BBCode format - ready to post in the forum
        /// </summary>
        protected override void MenuStrategyBBcode_OnClick(object sender, EventArgs e)
        {
            Strategy_Publish publisher = new Strategy_Publish();
            publisher.Show();

            return;
        }

        /// <summary>
        /// Tools menu
        /// </summary>
        protected override void MenuTools_OnClick(object sender, EventArgs e)
        {
            string sName = ((ToolStripMenuItem)sender).Name;

            switch (sName)
            {
                case "Reset settings":
                    ResetSettings();
                    break;
                case "miResetTrader":
                    ResetTrader();
                    break;
                case "miInstallExpert":
                    InstallMTFiles();
                    break;
                case "miNewTranslation":
                    MakeNewTranslation();
                    break;
                case "miEditTranslation":
                    EditTranslation();
                    break;
                case "miShowEnglishPhrases":
                    Language.ShowPhrases(1);
                    break;
                case "miShowAltPhrases":
                    Language.ShowPhrases(2);
                    break;
                case "miShowAllPhrases":
                    Language.ShowPhrases(3);
                    break;
                case "miOpenIndFolder":
                    try { System.Diagnostics.Process.Start(Data.SourceFolder); }
                    catch (System.Exception ex) { MessageBox.Show(ex.Message); }
                    break;
                case "miReloadInd":
                    Cursor = Cursors.WaitCursor;
                    ReloadCustomIndicators();
                    Cursor = Cursors.Default;
                    break;
                case "miCheckInd":
                    Custom_Indicators.TestCustomIndicators();
                    break;
                case "CommandConsole":
                    ShowCommandConsole();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Installs MT Expert and Library files.
        /// </summary>
        void InstallMTFiles()
        {
            try
            {
                System.Diagnostics.Process.Start(Data.ProgramDir + @"\MetaTrader\Install MT Files.exe");
            }
            catch { }

            return;
        }

        int sellOrBuy(int sellOrBuy)
        {
            if ((Data.AccountBalance * 0.1 * 0.001) == 0) return -1;

            string symbol = Data.Symbol;
            double lots =  double.Parse((Data.AccountBalance * 0.1*0.001).ToString("#.##"));
            double price = Data.Ask;
            int slippage = Configs.AutoSlippage ? (int)Data.InstrProperties.Spread * 3 : Configs.SlippageEntry;

            int stopLossPips = 0;
            if (OperationStopLoss > 0 && OperationTrailingStop > 0)
                stopLossPips = Math.Min(OperationStopLoss, OperationTrailingStop);
            else
                stopLossPips = Math.Max(OperationStopLoss, OperationTrailingStop);

            double stopLoss = price - 40.0 / 1000;
            double TakeProfit = price + 50.0 / 1000;

            if (Configs.PlaySounds)
                Data.SoundOrderSent.Play();

            string parameters = "TS1=" + OperationTrailingStop + ";BRE=" + OperationBreakEven;

            int response = -1;
            if (sellOrBuy == 1)
                response = bridge.OrderSend(symbol, OrderType.Buy, lots, Data.Ask, slippage, 400, 500, parameters);
            else if (sellOrBuy == 0)
                response = bridge.OrderSend(symbol, OrderType.Sell, lots, Data.Bid, slippage, 500, 400, parameters);
            else if (sellOrBuy == 2)//平仓
            {
               bool isok= DoExitTrade();
           
               if (isok) return 1;
            }

            if (response >= 0)
            {
                Data.AddBarStats(OperationType.Buy, lots, price);
                Data.WrongStopLoss = 0;
                Data.WrongTakeProf = 0;
                Data.WrongStopsRetry = 0;
            }
            else
            {   // Error in operation execution.
                if (Configs.PlaySounds)
                    Data.SoundError.Play();
               
                Data.WrongStopLoss = stopLossPips;
                Data.WrongTakeProf = OperationTakeProfit;
            }

            return response;
        }
       
        private Socket socket = null;
        private Thread thread = null;
        /// 
        /// 开始监听客户端
        /// 
        void StartListening()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                string serverIp = System.Configuration.ConfigurationManager.AppSettings["serverIp"];
                string port = System.Configuration.ConfigurationManager.AppSettings["port"];
                IPAddress ipaddress = IPAddress.Parse(serverIp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, int.Parse(port));

                socket.Bind(endPoint);
                socket.Listen(20);

                thread = new Thread(new ThreadStart(WatchConnection));
                thread.IsBackground = true;
                thread.Start();               
            }
            catch (System.Exception ex)
            {
               
            }
        }

        Socket[] socConnection = new Socket[12];
        private static int clientNum = 0;

        /// <summary>
        /// 监听客户端发来的请求
        /// </summary>
        private void WatchConnection()
        {
            while (IsRun)
            {
                Socket clientSocket = socket.Accept();
                Thread receiveThread = new Thread(ServerRecMsg);
                receiveThread.Start(clientSocket); 
            }
        }

        /// <summary>
        /// 接受客户端消息并发送消息
        /// </summary>
        /// <param name="socketClientPara"></param>
        private void ServerRecMsg(object socketClientPara)
        {
            Socket socketServer = socketClientPara as Socket;

            while (IsRun)
            {
                try
                {
                    byte[] arrServerRecMsg = new byte[1024 * 1024];
                    int length = socketServer.Receive(arrServerRecMsg);

                    string receiveString = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);

                    int res = -1;

                    if (receiveString.IndexOf("buy") > -1 || receiveString.IndexOf("sell") > -1)
                    {
                        if (receiveString.IndexOf("buy") > -1)
                        {
                            res = sellOrBuy(1);
                        }
                        else if (receiveString.IndexOf("sell") > -1)
                        {
                            res = sellOrBuy(0);
                        }

                        string returnMsg = "";
                        if (res > -1) returnMsg = "ok";
                        else returnMsg = "no";

                        byte[] arrSendMsg = Encoding.UTF8.GetBytes(returnMsg);
                        //发送消息到客户端
                        socketServer.Send(arrSendMsg);    
                    }
                    else if (receiveString.IndexOf("lots") > -1)//获取当前订单状态
                    {
                        byte[] arrSendMsg = Encoding.UTF8.GetBytes(msg);
                        //发送消息到客户端
                        socketServer.Send(arrSendMsg);  
                    }

                    //发出消息后退出                   
                    socketServer.Shutdown(SocketShutdown.Both);
                    socketServer.Close();
                    break;
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    socketServer.Shutdown(SocketShutdown.Both);
                    socketServer.Close();
                    break;
                }
            }
        }

    
        Thread td;
        private System.Threading.Timer timerClose;
        protected override void BtnOperation_Click(object sender, EventArgs e)
        {
            

            Button btn = sender as Button;
            btn.Enabled = false;
            td = new Thread(new ThreadStart(StartListening));
            td.Start();
            IsRun = true;

            timerClose = new System.Threading.Timer(new TimerCallback(timerCall), this, 0, 2000);
            timerClose.Change(0, 10000);

           // MessageBox.Show(Data.AccountBalance.ToString() + "   " + (Data.AccountBalance * 0.1*0.001).ToString("#.##"));
        }

        bool isLoad = false;
        List<DateTime> listDt = new List<DateTime>();
        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Document.Window.Error += Window_Error;

            if (!isLoad)
            {
                HtmlElement ele = this.webBrowser1.Document.GetElementById("current_data");

                if (null == ele) return;
                listDt.Clear();                

                HtmlElementCollection eleCol = ele.GetElementsByTagName("tr");
                foreach (HtmlElement tr in eleCol)
                {
                    bool isE = IsEurHigh(tr);
                    if (isE)//重要性 高
                    {
                        HtmlElementCollection eleColTd = tr.GetElementsByTagName("td");

                        foreach (HtmlElement td in eleColTd)
                        {
                            DateTime dt = DateTime.Now;

                            bool isTime = DateTime.TryParse(td.InnerText, out dt);
                            if (isTime)
                            {
                                if (dt.Hour < 15) continue;//15点才开始交易

                                if (!listDt.Contains(dt)) listDt.Add(dt);
                            }
                            break;//第一列是时间
                        }
                    }
                }
                isLoad = true;
                //MessageBox.Show(listDt.Count.ToString());
            }
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 是否是欧盟的高重要性
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        bool IsEurHigh(HtmlElement tr)
        {
            bool isB = false;

            List<string> listEurUSD = new List<string>();
            listEurUSD.Add("欧盟"); listEurUSD.Add("欧元区"); listEurUSD.Add("美国");
            listEurUSD.Add("奥地利"); listEurUSD.Add("比利时"); listEurUSD.Add("保加利亚");
            listEurUSD.Add("塞浦路斯"); listEurUSD.Add("克罗地亚"); listEurUSD.Add("捷克共和国");
            listEurUSD.Add("丹麦"); listEurUSD.Add("爱沙尼亚"); listEurUSD.Add("芬兰");
            listEurUSD.Add("法国"); listEurUSD.Add("德国"); listEurUSD.Add("希腊");
            listEurUSD.Add("匈牙利"); listEurUSD.Add("爱尔兰"); listEurUSD.Add("意大利");
            listEurUSD.Add("拉脱维亚"); listEurUSD.Add("立陶宛"); listEurUSD.Add("卢森堡");
            listEurUSD.Add("马耳他"); listEurUSD.Add("荷兰"); listEurUSD.Add("波兰");
            listEurUSD.Add("葡萄牙"); listEurUSD.Add("罗马尼亚"); listEurUSD.Add("斯洛伐克");
            listEurUSD.Add("斯洛文尼亚"); listEurUSD.Add("西班牙"); listEurUSD.Add("瑞典");

            if (tr.OuterHtml.IndexOf("red_color_s") > -1)
            {
                foreach (string country in listEurUSD)
                {
                    if (tr.OuterHtml.IndexOf(country) > -1)
                    {
                        isB = true;
                        break;
                    }
                }
            }

            return isB;
        }
        
        void timerCall(object obj)
        {
            string strDateTime = DateTime.Now.ToLongDateString() + " 00:29:00";

            DateTime closeTime = Convert.ToDateTime(strDateTime);

            if (DateTime.Now>closeTime)
            {
                sellOrBuy(2);  
            }

            //数据出现前5分钟 平仓
            if (this.listDt.Count > 0)
            {
                foreach (DateTime kv in this.listDt)
                {
                    if ((kv - DateTime.Now).Minutes < 5 && (kv - DateTime.Now).Minutes > 0)
                    {
                         
                          sellOrBuy(2);//平仓
                          //MessageBox.Show("平仓了");
                        break;
                    }
                }
            }

            //初始化财经日历
            if (this.listDt.Count > 0 && this.listDt[0].Month == DateTime.Now.Month && this.listDt[0].Day == DateTime.Now.Day)
                return; //一天初始化一次

            string strDateTime1 = DateTime.Now.ToLongDateString() + " 15:10:00";
            string strDateTime2 = DateTime.Now.ToLongDateString() + " 15:15:00";

            //string strDateTime1 = DateTime.Now.ToLongDateString() + " 20:15:00";
            //string strDateTime2 = DateTime.Now.ToLongDateString() + " 20:59:00";

            DateTime dateTime1 = Convert.ToDateTime(strDateTime1);
            DateTime dateTime2 = Convert.ToDateTime(strDateTime2);
            if (DateTime.Now > dateTime1 && DateTime.Now < dateTime2)
            {
                this.Invoke(new Action(() =>
                {
                    if (this.webBrowser1.ReadyState== WebBrowserReadyState.Complete
                        ||this.webBrowser1.ReadyState == WebBrowserReadyState.Uninitialized)
                    {
                        DateTime nowDt = DateTime.Now;
                        string monthStr = nowDt.Month > 10 ? nowDt.Month.ToString() : "0" + nowDt.Month.ToString();
                        string dayStr = nowDt.Day > 10 ? nowDt.Day.ToString() : "0" + nowDt.Day.ToString();
                        string nowStr = nowDt.Year.ToString() + monthStr + dayStr;
                        this.webBrowser1.Url = new Uri("http://rl.fx678.com/date/" + nowStr + ".html");

                        isLoad = false;
                        this.webBrowser1.DocumentCompleted -= webBrowser1_DocumentCompleted;
                        this.webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
                    }
                }));  
            }

           
        }

        public delegate void Action();

        /// <summary>
        /// Use logical groups menu item.
        /// </summary>
        protected override void MenuUseLogicalGroups_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            if (mi.Checked == true)
            {
                Configs.UseLogicalGroups = mi.Checked;
                RebuildStrategyLayout();
                return;
            }

            // Check if the current strategy uses logical groups
            bool usefroup = false;
            List<string> closegroup = new List<string>();
            foreach (IndicatorSlot slot in Data.Strategy.Slot)
            {
                if (slot.SlotType == SlotTypes.OpenFilter && slot.LogicalGroup != "A")
                    usefroup = true;

                if (slot.SlotType == SlotTypes.CloseFilter)
                {
                    if (closegroup.Contains(slot.LogicalGroup) || slot.LogicalGroup == "all")
                        usefroup = true;
                    else
                        closegroup.Add(slot.LogicalGroup);
                }
            }

            if (!usefroup)
            {
                Configs.UseLogicalGroups = false;
                RebuildStrategyLayout();
            }
            else
            {
                MessageBox.Show(
                    Language.T("The strategy requires logical groups.") + Environment.NewLine +
                    Language.T("\"Use Logical Groups\" option cannot be switched off."),
                    Language.T("Logical Groups"),
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                mi.Checked = true;
            }
            
            return;
        }

        /// <summary>
        /// Menu MenuOpeningLogicSlots_OnClick
        /// </summary>
        protected override void MenuOpeningLogicSlots_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            Configs.MAX_ENTRY_FILTERS = (int)mi.Tag;

            foreach (ToolStripMenuItem m in mi.Owner.Items)
                m.Checked = ((int)m.Tag == Configs.MAX_ENTRY_FILTERS);

            RebuildStrategyLayout();
            return;
        }

        /// <summary>
        /// Menu MenuClosingLogicSlots_OnClick
        /// </summary>
        protected override void MenuClosingLogicSlots_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            Configs.MAX_EXIT_FILTERS = (int)mi.Tag;

            foreach (ToolStripMenuItem m in mi.Owner.Items)
                m.Checked = ((int)m.Tag == Configs.MAX_EXIT_FILTERS);

            RebuildStrategyLayout();
            return;
        }

        /// <summary>
        /// Reset settings
        /// </summary>
        void ResetSettings()
        {
            DialogResult result = MessageBox.Show(
                Language.T("Do you want to reset all settings?") + Environment.NewLine + Environment.NewLine +
                Language.T("Restart the program to activate the changes!"),
                Language.T("Reset Settings"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
                Configs.ResetParams();
        }

        /// <summary>
        /// Reset data and stats.
        /// </summary>
        void ResetTrader()
        {
            tickLocalTime = DateTime.Now; // Prevents ping for one second.
            StopTrade();
            Data.IsConnected = false;

            bridge.ResetBarsManager();

            Data.ResetBidAsk();
            Data.ResetAccountStats();
            Data.ResetPositionStats();
            Data.ResetBarStats();
            Data.ResetTicks();

            UpdateTickChart(Data.InstrProperties.Point, Data.ListTicks.ToArray());
            UpdateBalanceChart(Data.BalanceData, Data.BalanceDataPoints);
            UpdateChart();

            return;
        }

        /// <summary>
        /// Starts the Calculator.
        /// </summary>
        void ShowCommandConsole()
        {
            Command_Console commandConsole = new Command_Console();
            commandConsole.Show();

            return;
        }

        /// <summary>
        /// Makes new language file.
        /// </summary>
        void MakeNewTranslation()
        {
            New_Translation nt = new New_Translation();
            nt.Show();

            return;
        }

        /// <summary>
        /// Edit translation.
        /// </summary>
        void EditTranslation()
        {
            Edit_Translation et = new Edit_Translation();
            et.Show();

            return;
        }
    }
}
