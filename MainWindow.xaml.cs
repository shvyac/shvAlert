using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace shvAlert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public static class ArrayExtensionMethods
    {
        public static ArraySegment<T> GetSegment<T>(this T[] arr, int offset, int? count = null)
        {
            if (count == null) { count = arr.Length - offset; }
            return new ArraySegment<T>(arr, offset, count.Value);
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Read_Call_Series();
            Read_Def_REGEXP();
        }

        List<CallSignAlloc> callsignseries = new List<CallSignAlloc>();
        List<CallSignRegExp> callsignregexp = new List<CallSignRegExp>();

        List<UDPModel.StatusModel> itemsStatusModel = new List<UDPModel.StatusModel>();
        List<UDPModel.DecodeModel> itemsDecodeModel = new List<UDPModel.DecodeModel>();
        private System.Net.Sockets.UdpClient udpClient = null;
        private DispatcherTimer _timer;
        //List<UDPModel.HeartbeatModel> itemsHeartbeatModel = new List<UDPModel.HeartbeatModel>();

        private void ButtonAlertConnect2Mshv_Click(object sender, RoutedEventArgs e)
        {
            //ListenBroadcastMessage();
            //RecieveUDPMessage();
            //DeserializeUDPMessage();
            //Task t = await OpenUDP(2237);

            DataGridAlertResult.ItemsSource = itemsDecodeModel;
            //ListViewAlertDebug.ItemsSource = itemsHeartbeatModel;

            SetupTimer();
            //Debug.WriteLine("ButtonAlertConnect2Mshv_Click SetupTimer");
            ListViewAlertDebug.Items.Add("ButtonAlertConnect2Mshv_Click SetupTimer");
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 15);
            _timer.Tick += new EventHandler(MyTimerMethod);
            _timer.Start();
            Debug.WriteLine("SetupTimer _timer.Start()");

            // 画面が閉じられるときに、タイマを停止
            this.Closing += new CancelEventHandler(StopTimer);
        }

        private void MyTimerMethod(object sender, EventArgs e)
        {
            Debug.WriteLine("MyTimerMethod Start");
            if (udpClient != null)
            {
                return;
            }
            //((Button)sender).IsEnabled = false;
            System.Net.IPEndPoint localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, int.Parse("2237"));
            udpClient = new System.Net.Sockets.UdpClient(localEP);

            DataGridAlertResult.Items.Refresh();

            //非同期的なデータ受信を開始
            udpClient.BeginReceive(ReceiveUDPCallback, udpClient);
        }

        private void StopTimer(object sender, CancelEventArgs e)// タイマを停止
        {
            _timer.Stop();
            Debug.WriteLine("StopTimer _timer.Stop()");
        }

        private void ButtonStopAlert_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Debug.WriteLine("ButtonStopAlert_Click _timer.Stop()");
        }

        public void Read_Call_Series()
        {
            string file_call_series = @"CallSignAllocation.txt";

            if (System.IO.File.Exists(file_call_series))
            {
                using (StreamReader sr = new StreamReader(file_call_series))
                {
                    Debug.WriteLine(@"file_call_series  = " + file_call_series);

                    string line;
                    int no_data = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        //Debug.WriteLine(line);

                        string[] fields = line.Split('\t');// 0=AAA-ABZ 1=Name

                        if (fields[0].Contains("-") && fields.Count() == 2)
                        {
                            //--------------------------------------------------------------Remove * and Split AAA-ABZ
                            string[] prefix = fields[0].Split('-');// 0=AAA 1=ABZ
                            if (prefix[0].Length == 3) { }
                            else
                            {
                                string str = prefix[0].Replace('*', ' ');
                                str = str.Trim();
                                prefix[0] = str;
                            }

                            //--------------------------------------------------------------Convert to ASCII Code. A=65 Z=90 0=48 9=57 Decimal
                            int[] PrefixStart = new int[3];
                            int[] PrefixEnd = new int[3];
                            for (int i = 0; i < 3; i++)
                            {
                                char[] cs = new char[] { 'A', 'B', 'C' };
                                char[] ce = new char[] { 'A', 'B', 'C' };
                                cs = prefix[0].ToCharArray();
                                ce = prefix[1].ToCharArray();

                                PrefixStart[i] = (int)cs[i];
                                PrefixEnd[i] = (int)ce[i];
                            }

                            //--------------------------------------------------------------Create callsignseries Table
                            for (int p1 = PrefixStart[0]; p1 <= PrefixEnd[0]; p1++)//First Letter         SSN-STZ	Sudan (Republic of the)
                            {
                                for (int p2 = PrefixStart[1]; p2 <= PrefixEnd[1]; p2++)//Second Letter
                                {
                                    if (PrefixStart[2] == 65 && PrefixEnd[2] == 90)//Third Letter      A-Z FULL
                                    {
                                        CallSignAlloc logItem = new CallSignAlloc()
                                        {
                                            strPrefix = ((char)p1).ToString() + ((char)p2).ToString(),
                                            strPrefix3 = @"*",
                                            strName = fields[1]
                                        };
                                        callsignseries.Add(logItem);
                                        no_data++;
                                        Debug.WriteLine(@"No.( {0} ) Start( {1} ) - End( {2} ) Name( {3} )", no_data, logItem.strPrefix, logItem.strPrefix3, logItem.strName);
                                    }
                                    else //A-Z Part
                                    {
                                        for (int p3 = PrefixStart[2]; p3 <= PrefixEnd[2]; p3++)
                                        {
                                            CallSignAlloc logItem = new CallSignAlloc()
                                            {
                                                strPrefix = ((char)p1).ToString() + ((char)p2).ToString(),
                                                strPrefix3 = ((char)p3).ToString(),
                                                strName = fields[1]
                                            };
                                            callsignseries.Add(logItem);
                                            no_data++;
                                            Debug.WriteLine(@"No.( {0} ) Start( {1} ) - End( {2} ) Name( {3} )", no_data, logItem.strPrefix, logItem.strPrefix3, logItem.strName);
                                        }
                                        PrefixStart[2] = 65;//A=65 Z=90
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine(@"field[0] is NOT contained - string , OR TAB NOT found:" + line);
                        }
                    }
                    ListViewAlertDebug.Items.Add(file_call_series + " File read normaly. Number of data: " + no_data);
                }
            }
            else
            {
                Debug.WriteLine(@"File not found:" + file_call_series);
            }
        }

        public void Read_Def_REGEXP()
        {
            string file_call_regexp = @"CallSignRegularExpression.txt";

            if (System.IO.File.Exists(file_call_regexp))
            {
                using (StreamReader sr = new StreamReader(file_call_regexp))
                {
                    Debug.WriteLine(@"file_call_regexp  = " + file_call_regexp);

                    string line;
                    int no_data = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Match matche = Regex.Match(line, @"\^.*\$");

                        if (0 < matche.Value.Length)
                        {
                            string[] fields = line.Split('$');
                            Debug.WriteLine("REGEXP: " + matche.Value + " ---> " + fields[1]);

                            CallSignRegExp regItem = new CallSignRegExp()
                            {
                                strRegExp = matche.Value,
                                strCountry = fields[1]
                            };
                            callsignregexp.Add(regItem);
                            no_data++;
                            Debug.WriteLine(@"No.( {0} ) RegExp( {1} ) - Country( {2} ) ", no_data, regItem.strRegExp, regItem.strCountry);
                        }
                        else
                        {
                            Debug.WriteLine("Comment: " + line);
                        }
                    }
                    ListViewAlertDebug.Items.Add(file_call_regexp + " File read normaly. Number of data: " + no_data);
                }
            }
            else
            {
                Debug.WriteLine(@"File not found:" + file_call_regexp);
            }
        }

        private void DispatcherMSG(string strMSG)
        {
            ListViewAlertDebug.Dispatcher.Invoke(() =>
                        {
                            ListViewAlertDebug.Items.Add(strMSG);
                            ListViewAlertDebug.Items.Refresh();

                            if (ListViewAlertDebug.Items.Count > 0)
                            {
                                var border = VisualTreeHelper.GetChild(ListViewAlertDebug, 0) as Decorator;
                                if (border != null)
                                {
                                    var scroll = border.Child as ScrollViewer;
                                    if (scroll != null) scroll.ScrollToEnd();
                                }
                            }
                        });
        }

        private void ReceiveUDPCallback(IAsyncResult ar)
        {
            System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)ar.AsyncState;

            //非同期受信を終了する
            System.Net.IPEndPoint remoteEP = null;
            byte[] rcvBytes;

            try
            {
                rcvBytes = udp.EndReceive(ar, ref remoteEP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Debug.WriteLine("受信エラー({0}/{1})", ex.Message, ex.ErrorCode);
                return;
            }
            catch (ObjectDisposedException ee)
            {
                Debug.WriteLine("Socketは閉じられています。");
                Debug.WriteLine("Caught: {0}", ee.Message);
                return;
            }

            string rcvMsg = System.Text.Encoding.UTF8.GetString(rcvBytes);
            string displayMsg = string.Format("[{0} ({1})] > {2}", remoteEP.Address, remoteEP.Port, rcvMsg);
            string rcvHEX = BitConverter.ToString(rcvBytes);

            Debug.WriteLine(rcvHEX);
            Debug.WriteLine(displayMsg);

            //-----------------------------------------------

            UDPMessageUtils nmu = new UDPMessageUtils();
            nmu.gIndex = 0;

            //UDPModel.HeartbeatModel hm;
            //UDPModel.StatusModel sm;
            UDPModel.DecodeModel dm;

            uint iMagic = nmu.Unpack4uint(rcvBytes, "magic");
            uint iSchema = nmu.Unpack4uint(rcvBytes, "schema");
            uint iMessageType = nmu.Unpack4uint(rcvBytes, "messageType");
            Debug.WriteLine("iMessageType {0}", iMessageType);

            switch (iMessageType)
            {
                case 0://Heartbeat Out/In 0 quint32

                    string id0 = nmu.Unpackstring(rcvBytes, "id");//Id (unique key) utf8
                    uint maxsch = nmu.Unpack4uint(rcvBytes, "maxscheme");//Maximum schema number quint32
                    string version = nmu.Unpackstring(rcvBytes, "version");//version utf8
                    string revision = nmu.Unpackstring(rcvBytes, "revision");//revision utf8

                    string hm = string.Format("{0} {1} {2} {3}", id0, maxsch, version, revision);

                    DispatcherMSG(hm);

                    break;

                case 1: //----------------------------------------------------------------------Status Out 1 quint32
                    string id1 = nmu.Unpackstring(rcvBytes, "id1");                         //Id (unique key) utf8

                    if (id1.Contains("MSHV"))
                    {
                        UInt64 DialFrequency = nmu.Unpack8uint(rcvBytes, "DialFrequency");      //Dial Frequency (Hz) quint64
                        string Mode = nmu.Unpackstring(rcvBytes, "Mode");                       //Mode utf8
                        string DXcall = nmu.Unpackstring(rcvBytes, "DXcall");                   //DX call utf8
                        string Report = nmu.Unpackstring(rcvBytes, "Report");                   //Report utf8
                        string TxMode = nmu.Unpackstring(rcvBytes, "TxMode");                   //Tx Mode utf8
                        bool TxEnbledBool = nmu.Unpackbool(rcvBytes, "TxEnbledBool");           //Tx Enabled bool
                        bool TransmittingBool = nmu.Unpackbool(rcvBytes, "TransmittingBool");   //Transmitting bool
                        bool DecodingBool = nmu.Unpackbool(rcvBytes, "DecodingBool");           //Decoding bool
                        int RxDF = nmu.Unpack4int(rcvBytes, "RxDF");                            //Rx DF qint32
                        int TxDF = nmu.Unpack4int(rcvBytes, "TxDF");                            //Tx DF qint32
                        string DEcall = nmu.Unpackstring(rcvBytes, "DEcall");                   //DE call utf8
                        string DEgrid = nmu.Unpackstring(rcvBytes, "DEgrid");                   //DE grid utf8
                        string DXgrid = nmu.Unpackstring(rcvBytes, "DXgrid");                   //DX grid utf8
                        bool TxWatchingBool = nmu.Unpackbool(rcvBytes, "TxWatchingBool");       //Tx Watchdog bool

                        string SubMode = nmu.Unpackstring(rcvBytes, "SubMode");                 //Sub-mode utf8
                        bool FastModeBool = nmu.Unpackbool(rcvBytes, "FastModeBool");           //Fast mode bool
                        int SpecialOpMode = nmu.Unpack1int(rcvBytes, "SpecialOpMode");          //Special operation mode quint8

                        string sm = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} ",
                            id1, DialFrequency, Mode, DXcall, Report, TxMode, TxEnbledBool, TransmittingBool, DecodingBool, RxDF, TxDF,
                            DEcall, DEgrid, DXgrid, TxWatchingBool, SubMode, FastModeBool, SpecialOpMode);

                        DispatcherMSG(sm);

                        //ListViewAlertDebug.Dispatcher.Invoke(() =>
                        //{
                        //    ListViewAlertDebug.Items.Add(sm);
                        //    Debug.WriteLine(ListViewAlertDebug.Items.Count);
                        //    ListViewAlertDebug.Items.Refresh();

                        //    if (ListViewAlertDebug.Items.Count > 0)
                        //    {
                        //        var border = VisualTreeHelper.GetChild(ListViewAlertDebug, 0) as Decorator;
                        //        if (border != null)
                        //        {
                        //            var scroll = border.Child as ScrollViewer;
                        //            if (scroll != null) scroll.ScrollToEnd();
                        //        }
                        //    }
                        //});
                    }
                    else if (id1.Contains("WSJT"))
                    {
                        UInt64 DialFrequency = nmu.Unpack8uint(rcvBytes, "DialFrequency");      //Dial Frequency (Hz) quint64
                        string Mode = nmu.Unpackstring(rcvBytes, "Mode");                       //Mode utf8
                        string DXcall = nmu.Unpackstring(rcvBytes, "DXcall");                   //DX call utf8
                        string Report = nmu.Unpackstring(rcvBytes, "Report");                   //Report utf8
                        string TxMode = nmu.Unpackstring(rcvBytes, "TxMode");                   //Tx Mode utf8
                        bool TxEnbledBool = nmu.Unpackbool(rcvBytes, "TxEnbledBool");           //Tx Enabled bool
                        bool TransmittingBool = nmu.Unpackbool(rcvBytes, "TransmittingBool");   //Transmitting bool
                        bool DecodingBool = nmu.Unpackbool(rcvBytes, "DecodingBool");           //Decoding bool
                        uint RxDF = nmu.Unpack4uint(rcvBytes, "RxDF");                          //Rx DF qint32 >>> quint32
                        uint TxDF = nmu.Unpack4uint(rcvBytes, "TxDF");                          //Tx DF qint32 >>> quint32
                        string DEcall = nmu.Unpackstring(rcvBytes, "DEcall");                   //DE call utf8
                        string DEgrid = nmu.Unpackstring(rcvBytes, "DEgrid");                   //DE grid utf8
                        string DXgrid = nmu.Unpackstring(rcvBytes, "DXgrid");                   //DX grid utf8
                        bool TxWatchingBool = nmu.Unpackbool(rcvBytes, "TxWatchingBool");       //Tx Watchdog bool
                        string SubMode = nmu.Unpackstring(rcvBytes, "SubMode");                 //Sub-mode utf8
                        bool FastModeBool = nmu.Unpackbool(rcvBytes, "FastModeBool");           //Fast mode bool
                        int SpecialOpMode = nmu.Unpack1int(rcvBytes, "SpecialOpMode");          //Special operation mode quint8

                        int FrequencyTolerance = nmu.Unpack4int(rcvBytes, "FrequencyTolerance");          //Frequency Tolerance quint32
                        int TRPeriod = nmu.Unpack4int(rcvBytes, "TRPeriod");                                  //T / R Period quint32
                        string ConfigurationName = nmu.Unpackstring(rcvBytes, "ConfigurationName");          //Configuration Name utf8

                        string sm = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} ",
                            id1, DialFrequency, Mode, DXcall, Report, TxMode, TxEnbledBool, TransmittingBool, DecodingBool, RxDF, TxDF,
                            DEcall, DEgrid, DXgrid, TxWatchingBool, SubMode, FastModeBool, SpecialOpMode, FrequencyTolerance, TRPeriod, ConfigurationName);

                        DispatcherMSG(sm);
                    }
                    else
                    {
                        Debug.WriteLine("*** Illegal id1 {0} Found. It is not MSHV and WSJT.", id1);
                    }

                    break;

                case 2: //----------------------------------------------------------------------Decode
                    string id2 = nmu.Unpackstring(rcvBytes, "id");
                    bool boNew = nmu.Unpackbool(rcvBytes, "isNew");
                    DateTime datetm = nmu.UnpackDateTime(rcvBytes, "tm");
                    int SNR = nmu.Unpack4int(rcvBytes, "snr");
                    float DT = nmu.Unpack8float(rcvBytes, "dt");
                    uint DF = nmu.Unpack4uint(rcvBytes, "df");
                    string MODE = nmu.Unpackstring(rcvBytes, "mode");
                    string Message = nmu.Unpackstring(rcvBytes, "message");

                    string[] strCountry = GetCountry(Message);

                    dm = new UDPModel.DecodeModel()
                    {
                        //heartbeat_client_id = iMagic,
                        //heartbeat_maximum_schema_number = iSchema,
                        decode_client_id = id2,
                        decode_new = boNew,
                        decode_time = datetm,
                        decode_snr = SNR,
                        decode_delta_time = Math.Round(DT, 1),
                        decode_delta_frequency = DF,
                        decode_mode = MODE,
                        decode_message = Message,
                        alloc_left = strCountry[0],
                        alloc_right = strCountry[1]
                    };

                    DataGridAlertResult.Dispatcher.Invoke(() =>
                    {
                        itemsDecodeModel.Add(dm);
                        Debug.WriteLine(itemsDecodeModel.Count);

                        DataGridAlertResult.ItemsSource = itemsDecodeModel;
                        DataGridAlertResult.Items.Refresh();
                        //DataGridAlertResult.UpdateLayout();

                        if (DataGridAlertResult.Items.Count > 0)
                        {
                            var border = VisualTreeHelper.GetChild(DataGridAlertResult, 0) as Decorator;
                            if (border != null)
                            {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) scroll.ScrollToEnd();
                            }
                        }
                        //string[] strCountry = GetCountry(Message);
                    });
                    break;

                default:
                    Debug.WriteLine("*** iMessageType {0} Found", iMessageType);
                    break;
            }

            //再びデータ受信を開始する
            udp.BeginReceive(ReceiveUDPCallback, udp);
            Debug.WriteLine("BeginReceive Again");
        }

        private void DataGridAlertResult_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            Style h_Right = new Style();
            h_Right.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Right));
            e.Column.CellStyle = h_Right;

            string headername = e.Column.Header.ToString();
            switch (headername)
            {
                case "decode_client_id":
                    e.Column.Header = "Client";
                    break;
                case "decode_new":
                    e.Column.Header = "New";
                    break;
                case "decode_time":
                    e.Column.Header = "UTC";
                    break;
                case "decode_snr":
                    e.Column.Header = "dB";
                    break;
                case "decode_delta_time":
                    e.Column.Header = "DT";
                    break;
                case "decode_delta_frequency":
                    e.Column.Header = "DF";
                    break;
                case "decode_mode":
                    e.Column.Header = "Mode";
                    break;
                case "decode_message":
                    e.Column.Header = "Message";
                    break;
                case "alloc_left":
                    e.Column.Header = "Call 1";
                    break;
                case "alloc_right":
                    e.Column.Header = "Call 2";
                    break;

                default:
                    e.Column.Header = "Default";
                    break;
            }
        }

        //public string[] GetCountry2(string decodedMSG)
        //{
        //    string[] retCountry = new string[2];
        //    return retCountry;
        //}

        public string[] GetCountry(string decodedMSG)// JA1AAA JH1SSS PM96
        {
            string[] retCountry = new string[2];
            string[] fields = decodedMSG.Split(' ');

            for (int i = 0; i < 2; i++)
            {
                string pre1 = fields[i].Replace("<", "");
                string pre2 = pre1.Substring(0, 2);
                if (pre2 == @"CQ")
                {
                    retCountry[i] = pre2;
                }
                else
                {
                    Debug.WriteLine(pre2);

                    if (callsignseries.Exists(x => x.strPrefix.Contains(pre2)))
                    {
                        retCountry[i] = callsignseries.Find(x => x.strPrefix == pre2).strName;
                        Debug.WriteLine(retCountry[i] + " exists in " + decodedMSG);

                        if (retCountry[i].Contains("REGEXP"))
                        {
                            string Province = GetCountryREGEXP(fields[i]);
                            retCountry[i] = Province;
                            Debug.WriteLine("REGEXP: " + Province + " " + fields[i]);
                        }
                    }
                    else
                    {
                        retCountry[i] = pre2 + " n/a";
                        Debug.WriteLine(pre2 + "  NOT FOUND in " + decodedMSG);
                    }
                }
                //Debug.WriteLine(retCountry[i]);     
            }
            return retCountry;
        }

        public string GetCountryREGEXP(string callSign)// RA0AA
        {
            foreach (var item in callsignregexp)
            {
                Match matche = Regex.Match(callSign, item.strRegExp);
                if (0 < matche.Value.Length)
                {
                    return item.strCountry;
                }
            }
            return "Province NOT found in " + callSign;
        }
    }
}

