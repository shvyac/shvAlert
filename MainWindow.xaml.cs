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

        private void DispatcherLABEL(Label labelName, string strMSG)
        {
            labelName.Dispatcher.Invoke(() =>
            {
                labelName.Content =strMSG;
            });
        }

        private string gMyGridLoc;

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

            //Debug.WriteLine(rcvHEX);
            //Debug.WriteLine(displayMsg);

            //-----------------------------------------------

            UDPMessageUtils nmu = new UDPMessageUtils();
            nmu.gIndex = 0;

            //UDPModel.HeartbeatModel hm;
            //UDPModel.StatusModel sm;
            UDPModel.DecodeModel dm;

            uint iMagic = nmu.Unpack4uint(rcvBytes, "magic");
            uint iSchema = nmu.Unpack4uint(rcvBytes, "schema");
            uint iMessageType = nmu.Unpack4uint(rcvBytes, "messageType");
            Debug.WriteLine("iMessageType = {0}", iMessageType);

            switch (iMessageType)
            {
                case 0://Heartbeat Out/In 0 quint32

                    string id0 = nmu.Unpackstring(rcvBytes, "id");              //Id (unique key) utf8
                    uint maxsch = nmu.Unpack4uint(rcvBytes, "maxscheme");       //Maximum schema number quint32
                    string version = nmu.Unpackstring(rcvBytes, "version");     //version utf8
                    string revision = nmu.Unpackstring(rcvBytes, "revision");   //revision utf8

                    string hm = string.Format("id {0}, maxscheme {1}, version {2}, revision{3}", id0, maxsch, version, revision);

                    DispatcherLABEL(LabelID, id0);
                    DispatcherLABEL(LabelMaximumSchemaNum, maxsch.ToString());
                    DispatcherLABEL(LabelVersion, version);
                    DispatcherLABEL(LabelRevision, revision);

                    //DispatcherMSG(hm);

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

                        string sm = string.Format(
                            "{0}, {1}, {2}, {3}, {4}, {5}," +
                            "TxEnbledBool {6}, TransmittingBool {7}, DecodingBool {8}, RxDF {9}, TxDF {10}," +
                            " {11}, {12}, {13}, TxWatchingBool {14}, SubMode {15}," +
                            "FastModeBool {16}, SpecialOpMode {17}",
                            id1, DialFrequency, Mode, DXcall, Report, TxMode, 
                            TxEnbledBool, TransmittingBool, DecodingBool, RxDF, TxDF, 
                            DEcall, DEgrid, DXgrid, TxWatchingBool, SubMode, 
                            FastModeBool, SpecialOpMode
                            );

                        gMyGridLoc = DEgrid;

                        DispatcherMSG(sm);

                        DispatcherLABEL(LabelDialFrequency, (DialFrequency/1000).ToString("#,0"));
                        DispatcherLABEL(LabelDXcall, DXcall);
                        DispatcherLABEL(LabelDEcall, DEcall);
                        DispatcherLABEL(LabelDXgrid, DXgrid);
                        DispatcherLABEL(LabelDEgrid, DEgrid);
                        DispatcherLABEL(LabelMode, Mode);
                        DispatcherLABEL(LabelTxMode, TxMode);
                        DispatcherLABEL(LabelReport, Report);
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

                        string sm = string.Format(
                            "{0}, DialFrequency {1}, Mode {2}, DXcall {3}, Report {4}, TxMode {5}," +
                            "TxEnbledBool {6}, TransmittingBool {7}, DecodingBool {8}, RxDF {9}, TxDF {10}," +
                            "DEcall {11}, DEgrid {12}, DXgrid {13}, TxWatchingBool {14}, SubMode {15}," +
                            "FastModeBool {16}, SpecialOpMode {17}, FrequencyTolerance {18}, TRPeriod {19}, ConfigurationName {20} ",
                            id1, DialFrequency, Mode, DXcall, Report, TxMode, TxEnbledBool, TransmittingBool, DecodingBool, RxDF, TxDF,
                            DEcall, DEgrid, DXgrid, TxWatchingBool, SubMode, FastModeBool, SpecialOpMode, FrequencyTolerance, TRPeriod, ConfigurationName
                            );

                        DispatcherMSG(sm);
                    }
                    else
                    {
                        DispatcherMSG(string.Format("*** Illegal id1 {0} Found. It is not MSHV and WSJT.", id1));
                    }

                    break;

                case 2: //----------------------------------------------------------------------Decode
                    string id2      = nmu.Unpackstring(rcvBytes, "id");
                    bool boNew      = nmu.Unpackbool(rcvBytes, "isNew");
                    DateTime datetm = nmu.UnpackDateTime(rcvBytes, "tm");
                    int SNR         = nmu.Unpack4int(rcvBytes, "snr");
                    float DT        = nmu.Unpack8float(rcvBytes, "dt");
                    uint DF         = nmu.Unpack4uint(rcvBytes, "df");
                    string MODE     = nmu.Unpackstring(rcvBytes, "mode");
                    string Message  = nmu.Unpackstring(rcvBytes, "message");

                    string[] strCountry = GetCountry(Message);  // Get Call Sign Allocation Info. strCountry[2]=Distance Km

                    dm = new UDPModel.DecodeModel()
                    {
                        //heartbeat_client_id = iMagic,
                        //heartbeat_maximum_schema_number = iSchema,
                        decode_client_id        = id2,
                        decode_new              = boNew,
                        decode_time             = datetm,
                        decode_snr              = SNR,
                        decode_delta_time       = Math.Round(DT, 1),
                        decode_delta_frequency  = DF,
                        decode_mode             = MODE,
                        decode_message          = Message,
                        alloc_left              = strCountry[0], // Left Call
                        alloc_right             = strCountry[1], // Right Call
                        distance_from_here      = strCountry[2],
                        distance_between_them   = @""
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
            //Debug.WriteLine("BeginReceive Again");
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
                case "distance_from_here":
                    e.Column.Header = "From Here Km";
                    break;
                case "distance_between_them":
                    e.Column.Header = "Between Km";
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

        public string[] GetCountry(string decodedMSG)
        {
            // CQ JA1AAA PM96
            // JA1AAA JA9SSS PM96
            // JA1AAA JA9SSS -10
            // JA1AAA JA9SSS R-10
            // JA1AAA JA9SSS 73
            // JA1AAA JA9SSS RR73
            // JA1AAA JA9SSS RRR

            //-----------------------------------------------------
            //string[] retCountry = new string[2];  // LeftCall RightCall

            string[] retCountry = new string[3];  // LeftCall RightCall RightCallDistance Km
            
            string[] fields = decodedMSG.Split(' ');
            int fieldsLength = fields.Length;
            if (fieldsLength < 2  || 4 < fieldsLength)
            {
                
            }
            switch (fieldsLength)
            {
                case 2://  JA1AAA JA9SSS
                    DispatcherMSG(decodedMSG + " --- Length 2");

                    Array.Resize(ref fields, fields.Length + 1);
                    fields[fields.Length - 1] = @"NULL";
                    retCountry[2] = @"Null GL";
                    break;

                case 3://  JA1AAA JA9SSS PM96
                    Debug.WriteLine("===> " + fields[2]); // PM96 73 RR73 RRR
                    break;

                case 4:
                    // CQ EU JA1AAA PM96
                    DispatcherMSG(decodedMSG + " --- Length 4");

                    var list = new List<string>();
                    list.AddRange(fields);
                    list.RemoveAt(1);
                    fields = list.ToArray();                    
                    break;

                default:
                    DispatcherMSG(decodedMSG + " --- Length " + fieldsLength);
                    break;
            }

            for (int i = 0; i < 2; i++)
            {
                string calli = CheckIllegalCall(fields[i]);
                string pre2 = calli.Substring(0, 2);

                if (pre2 == @"CQ")
                {
                    retCountry[i] = pre2;
                }
                else
                {
                    Debug.Write(pre2 + " in " + calli + " " + i + ", ");

                    if (callsignseries.Exists(x => x.strPrefix.Contains(pre2)))
                    {
                        retCountry[i] = callsignseries.Find(x => x.strPrefix == pre2).strName;
                        Debug.WriteLine(retCountry[i] + " exists in callsignseries, " + decodedMSG);

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
                        string s = decodedMSG + ", Prefix " + pre2 + " NOT FOUND in callsignseries";
                        Debug.WriteLine(s);
                        DispatcherMSG(s);
                    }
                }
                //Debug.WriteLine(retCountry[i]);     
            }

            //Debug.WriteLine("===> " + fields[2]);
            Match match2 = Regex.Match(fields[2], @"[A-R]{2}[0-9]{2}");
            if (0 < match2.Value.Length)
            {
                if (fields[2] != @"RR73")
                {
                    int dist = GetDistanceBetween(fields[2], gMyGridLoc);       // Get Distance Between 
                    retCountry[2] = dist.ToString();
                }
                else
                {
                    retCountry[2] = @"";
                }
            }

            return retCountry;
        }

        public string CheckIllegalCall(string callSign)// <RA0AA> JA1AAA/M 
        {
            int orgLength = callSign.Length;
            string orgCall = callSign;

            if (callSign.Contains(@"<")) callSign = callSign.Replace(@"<", @"");
            if (callSign.Contains(@">")) callSign = callSign.Replace(@">", @"");
            if (callSign.Contains(@"/")) callSign = callSign.Substring(0, callSign.LastIndexOf(@"/")-1);
            if (orgLength != callSign.Length)
            {
                DispatcherMSG(@"CheckIllegalCall Modified: " + orgCall + " to " + callSign);
            }
            return callSign;
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

        public int GetDistanceBetween(string DxGrid, string DeGrid)
        {
            //int[] retCountry = new int[2];
            //DxGrid = @"NO65";
            //DeGrid = @"PM96";

            char[] cdx = DxGrid.ToCharArray();
            char[] cde = DeGrid.ToCharArray();

            double lonDX = ((int)cdx[0] - 65) * 20.0 + Convert.ToInt32(cdx[2].ToString()) * 2.0 +1.0   - 180.0;
            double latDX  = ((int)cdx[1] - 65) * 10.0 + Convert.ToInt32(cdx[3].ToString())  + 0.5 - 90.0;

            double lonDE = ((int)cde[0] - 65) * 20.0 + Convert.ToInt32(cde[2].ToString()) * 2.0 +1.0  - 180.0;
            double latDE = ((int)cde[1] - 65) * 10.0 + Convert.ToInt32(cde[3].ToString()) + 0.5  - 90.0;

            //int longitude =(CODE(MID(A1,1,1))-65)*20 + VALUE(MID(A1,3,1))*2 + (CODE(MID(A1,5,1))-97)/12 + 1/24 - 180
            //int latitude  =(CODE(MID(A1, 2, 1)) - 65) * 10 + VALUE(MID(A1, 4, 1)) + (CODE(MID(A1, 6, 1)) - 97) / 24 + 1 / 48 - 90;
            //--------------------------------------------------------------Convert to ASCII Code. A=65 Z=90 0=48 9=57 Decimal

            /*
            Assuming for the sake of precision that the particular point in the grid square 
            that you want the exact latitude and longitude for is the midpoint of the 6-character subsquare, 
            this can be done readily with Excel formulas.
            If the 6-character grid square data is in cell A1, in a format similar to AA00aa (i.e. upper-case, then digits, then lower-case), 
            the formula for the latitude (based directly on the Python code posted previously) is:

            =(CODE(MID(A1,2,1))-65)*10 + VALUE(MID(A1,4,1)) + (CODE(MID(A1,6,1))-97)/24 + 1/48 - 90

            and the formula for the longitude is

            =(CODE(MID(A1,1,1))-65)*20 + VALUE(MID(A1,3,1))*2 + (CODE(MID(A1,5,1))-97)/12 + 1/24 - 180

            If you want the latitude and longitude of the southwest corner of the subsquare, just leave out the + 1/48 and + 1/24 terms. 
            Add error-checking, upper- and lower-case conversion, conversion of four-character squares to six-character by adding 'mm', 
            and other embellishments as you see fit.
            */

            return CalcDistance(latDX, latDE, lonDX, lonDE);
        }

        public int CalcDistance(double lat1, double lat2, double lon1, double lon2)
        {
            double R = 6371000;
            double phi1 = ToRadian(lat1);
            double phi2 = ToRadian(lat2);
            double deltaPhi = ToRadian(lat2 - lat1);
            double deltaLambda = ToRadian(lon2 - lon1);

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c / 1000.0 ; //Km

            /*
            var R = 6371e3; // metres
            var φ1 = lat1.toRadians();
            var φ2 = lat2.toRadians();
            var Δφ = (lat2-lat1).toRadians();
            var Δλ = (lon2-lon1).toRadians();
            var a = Math.sin(Δφ/2) * Math.sin(Δφ/2) +
                    Math.cos(φ1) * Math.cos(φ2) *
                    Math.sin(Δλ/2) * Math.sin(Δλ/2);
            var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
            var d = R * c;
            */

            return (int)d;  //Km
        }

        public double ToRadian(double angle)
        {
            return (double)(angle * Math.PI / 180);
        }
    }
}

