using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HslCommunication.Profinet;
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Media;

namespace FreezePunch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public class LogToDisplay : INotifyPropertyChanged
    {
        private string text; 
        public string Text

        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
}
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;
        const uint ES_CONTINUOUS = 0x80000000;

        private int Exp_State;

        private DispatcherTimer ShowTimer;
        private DispatcherTimer LabelTimer;
        private DateTime StartTime;
        private DateTime SpanDateTime;
        private TimeSpan Span;
        private Thread t;

        private bool COM_Connectted = false;
        private MelsecFxSerial melsecSerial = null;
        private int pre_label_time;
        private int sample_1;
        private int sample_2;
        private int sample_3;
        private int sample_4;
        private bool flag_1;
        private bool flag_2;
        private bool flag_3;
        private bool flag_4;
        private int cycle_count;
        private Thread RefreshThread;
        private DateTime time_1;
        private DateTime time_2;
        private DateTime time_3;
        private DateTime time_4;

        private LogToDisplay log_context;

        // private SoundPlayer beep;

        public MainWindow()
        {
            InitializeComponent();

            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

            
            Exp_State = 0;
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//时钟timer
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            LabelTimer = new System.Windows.Threading.DispatcherTimer(); //标记Timer
            LabelTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            LabelTimer.Tick += new EventHandler(LabelTimer_Tick);

            pre_label_time = 0;
            sample_1 = 0;
            sample_2 = 60;
            sample_3 = 180;
            sample_4 = 420;
            flag_1 = flag_1 = flag_3 = flag_4 = false;
            cycle_count = 0;

            PRE_LABEL.Text = pre_label_time.ToString();
            SAMPLE_1.Text = sample_1.ToString();
            SAMPLE_2.Text = sample_2.ToString();
            SAMPLE_3.Text = sample_3.ToString();
            SAMPLE_4.Text = sample_4.ToString();

            //beep = new SoundPlayer("Resources/beep.wav");
            

            string[] ArrayComPortsName = SerialPort.GetPortNames(); //获取当前串口个数名称
            if (ArrayComPortsName.Length != 0)
            {
                Array.Sort(ArrayComPortsName);
                for (int i = 0; i < ArrayComPortsName.Length; i++)
                {
                    COM_LIST.Items.Add(ArrayComPortsName[i]);
                }
                if (ArrayComPortsName.Length == 1)
                {
                    COM_LIST.SelectedIndex = 0;
                }
            }
            log_context = new LogToDisplay();
            log_context.Text = "[" + DateTime.Now.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss") + "]" + "Terminal online, welcome.\r\n";
            LOG.DataContext = log_context;
        }
        private void LabelTimer_Tick(object sender, EventArgs e)
        {
            //const double EPS = 1e-6;
            Span = DateTime.Now - StartTime;       //Use system time to calcuate the TimeSpan
            SpanDateTime = new DateTime(Span.Ticks);
            TimeSpan next_sample = new TimeSpan(0);
            Labelling_Time.Content = SpanDateTime.ToString("mm:ss.ff");
            if (SpanDateTime < time_1)
                next_sample = time_1 - SpanDateTime;
            if (time_1 < SpanDateTime & SpanDateTime < time_2)
                next_sample = time_2 - SpanDateTime;
            if (time_2 < SpanDateTime & SpanDateTime < time_3)
                next_sample = time_3 - SpanDateTime;
            if (time_3 < SpanDateTime & SpanDateTime < time_4)
                next_sample = time_4 - SpanDateTime;
            Next_Sampling.Content = next_sample.ToString(@"mm\:ss\.ff");
            /*TimeSpan ts = new TimeSpan(next_sample);
            if (ts.TotalSeconds <= 10.0 && (ts.TotalSeconds - Math.Floor(ts.TotalSeconds) < EPS))
            {
                beep.Play();
            }*/
            if (SpanDateTime >= time_1 & !flag_1)
            {
                Samplling(melsecSerial, "M501");
                flag_1 = true;
                log_context.Text += "[" + TM.Text + "]" + "SAMPPLING: 1/4.\r\n";
            }
            if (SpanDateTime >= time_2 & !flag_2)
            {
                Samplling(melsecSerial, "M502");
                flag_2 = true;
                log_context.Text += "[" + TM.Text + "]" + "SAMPPLING: 2/4.\r\n";
            }
            if (SpanDateTime >= time_3 & !flag_3)
            {
                Samplling(melsecSerial, "M503");
                flag_3 = true;
                log_context.Text += "[" + TM.Text + "]" + "SAMPPLING: 3/4.\r\n";
            }
            if (SpanDateTime >= time_4 & !flag_4)
            {
                Samplling(melsecSerial, "M504");
                flag_4 = true;
                melsecSerial.Write("M500", false);
                Exp_State = 0;
                LabelTimer.Stop();
                LabelTimer.IsEnabled = false;
                ShowTimer.Start();
                log_context.Text += "[" + TM.Text + "]" + "SAMPPLING: 4/4.\r\n";
                this.START.Content = "START";
                this.START.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                log_context.Text += "[" + TM.Text + "]" + " Labelling FINISH.\r\n";
                flag_1 = flag_2 = flag_3 = flag_4 = false;
            }
            cycle_count += 1;
            if (cycle_count == 10)
            {
                ShowCurTimer(sender,e);
                cycle_count = 0;
            }
        }

        public void RefreshData()
        {
            while (true)
            {
                UInt32 MFC_N2_raw = 0;
                UInt32 MFC_CO2_raw = 0;
                UInt32 RH_raw = 0;
                UInt32 MFM_raw = 0;
                bool label_sw = false;
                bool RefreshSuccess = true;
                if (melsecSerial.ReadUInt16("D100").IsSuccess)
                {
                    MFC_N2_raw = melsecSerial.ReadUInt16("D100").Content;
                }
                else
                {
                    RefreshSuccess = false;
                }
                if (melsecSerial.ReadUInt16("D101").IsSuccess)
                {
                    MFC_CO2_raw = melsecSerial.ReadUInt16("D101").Content;
                }
                else
                {
                    RefreshSuccess = false;
                }
                if (melsecSerial.ReadUInt16("D102").IsSuccess)
                {
                    RH_raw = melsecSerial.ReadUInt16("D102").Content;
                }
                else
                {
                    RefreshSuccess = false;
                }
                if (melsecSerial.ReadUInt16("D103").IsSuccess)
                {
                    MFM_raw = melsecSerial.ReadUInt16("D103").Content;
                }
                else
                {
                    RefreshSuccess = false;
                }
                if (melsecSerial.ReadBool("M500").IsSuccess)
                {
                    label_sw = melsecSerial.ReadBool("M500").Content;
                }
                else
                {
                    RefreshSuccess = false;
                }
                //log_context.Text += "[" + TM.Text + "]" + MFC_N2_raw+","+MFC_CO2_raw + "\r\n";
                //
                double MFC_N2_float = (float)MFC_N2_raw / 4000 * 400;
                double MFC_CO2_float = (float)(MFC_CO2_raw) / 4000 * 30;
                double MFM_float = (float)(MFM_raw -383)/ 3200 * 30;
                double RH_real = (float)RH_raw / 4000 * 5 / 3 * 100;   //0-3V 0-99.9 %RH
                double Total_Flow = MFC_N2_float + MFC_CO2_float / 1000;
                MFC_N2_RT.Dispatcher.Invoke(new Action(delegate
                {
                    this.MFC_N2_RT.Content = (MFC_N2_float).ToString("#.0");
                }));
                    
                MFC_CO2_RT.Dispatcher.Invoke(new Action(delegate
                {
                    this.MFC_CO2_RT.Content = (MFC_CO2_float).ToString("#.00");
                }));

                FLOW.Dispatcher.Invoke(new Action(delegate
                {
                    this.FLOW.Content = (Total_Flow).ToString("#.0"); //400 SLLM, 30 SCCM
                }));
                RH.Dispatcher.Invoke(new Action(delegate
                {
                    this.RH.Content = (RH_real).ToString("#.0"); 
                }));
                RH.Dispatcher.Invoke(new Action(delegate
                {
                    this.MFM_CO2_RT.Content = (MFM_float).ToString("#.00");
                }));
                PPM.Dispatcher.Invoke(new Action(delegate
                {
                    if (MFC_N2_raw != 0)
                    {
                        this.PPM.Content = (1000 * MFC_CO2_float / Total_Flow).ToString("#.0");
                    }
                    else
                    {
                        PPM.Content = "N.A.";
                    }
                }));
                LABEL_SW.Dispatcher.Invoke(new Action(delegate
                {
                    LABEL_SW.IsChecked = label_sw;
                }));
                if (!RefreshSuccess)
                {
                    melsecSerial.Close();
                    RefreshThread.Abort();
                    COM_Connectted = false;
                    COM_CONNECT.Dispatcher.Invoke(new Action(delegate
                    {
                        COM_CONNECT.Content = "CONNECT"; 
                    }));
                    MessageBox.Show("Device is offline.");
                }
                Thread.Sleep(500);
            }

        }


        public void ShowCurTimer(object sender, EventArgs e)
        {
            //"星期"+DateTime.Now.DayOfWeek.ToString(("d"))
            //获得星期几
            //this.TM.Text = DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("zh-cn"));
            //this.TM.Text += " ";
            //获得年月日
            //TM.Text = DateTime.Now.ToString("yyyy/MM/dd");   //yyyy年MM月dd日
            //TM.Text += "\n";
            //获得时分秒
            TM.Text = DateTime.Now.ToString("HH:mm:ss");
            //System.Diagnostics.Debug.Print("this.ShowCurrentTime {0}", this.ShowCurrentTime);
        }

        private void LED_Volt_Enter(object sender, KeyEventArgs e)
        {
            if (COM_Connectted & e.Key == Key.Enter)
            {
                Int32 value = (Int32)(ObjectToFloat(LED_Volt.Text) * 40);
                melsecSerial.Write("D200", value);
                melsecSerial.Write("D201", value);
                melsecSerial.Write("D202", value);
                melsecSerial.Write("D203", value);
                log_context.Text += "[" + TM.Text + "]" + "LED is set to: " + value + ".\r\n";
            }
                
        }

        private void MFC_N2_Set_Enter(object sender, KeyEventArgs e)
        {
            if (COM_Connectted & e.Key == Key.Enter)
            {
                Int32 value = (Int32)(ObjectToFloat(MFC_N2_SET.Text) /400 * 4000);
                melsecSerial.Write("D300", value);
                log_context.Text += "[" + TM.Text + "]" + "MFC(N2) is set to: " + value + ".\r\n";
            }

        }

        private void MFC_CO2_Set_Enter(object sender, KeyEventArgs e)
        {
            if (COM_Connectted & e.Key == Key.Enter)
            {
                Int32 value = (Int32)(ObjectToFloat(MFC_CO2_SET.Text) / 30 * 4000);
                melsecSerial.Write("D400", value);
                log_context.Text += "[" + TM.Text + "]" + "MFC(CO2) is set to: " + value + ".\r\n";
            }
        }

        private void Pre_Labelling_Time_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pre_label_time = Convert.ToInt32(PRE_LABEL.Text);
                log_context.Text += "[" + TM.Text + "]" + "Pre labelling time is set to: " + MFC_N2_SET.Text + "s.\r\n";
            }
        }

        private void START_Click(object sender, RoutedEventArgs e)
        {
            if (COM_Connectted)
            {
                if (Exp_State == 0)
                {
                    this.START.Content = "ABORT";
                    this.START.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));
                    log_context.Text += "[" + TM.Text + "]" + " Labelling START.\r\n";

                    sample_1 += pre_label_time;
                    sample_2 += pre_label_time;
                    sample_3 += pre_label_time;
                    sample_4 += pre_label_time;
                    time_1 = new DateTime((long)sample_1 * 10000000);
                    time_2 = new DateTime((long)sample_2 * 10000000);
                    time_3 = new DateTime((long)sample_3 * 10000000);
                    time_4 = new DateTime((long)sample_4 * 10000000);
                    
                    Exp_State = 1;
                    melsecSerial.Write("M500", true);
                    StartTime = DateTime.Now;

                    if (sample_1 == 0 & !flag_1)
                    {
                        Samplling(melsecSerial, "M501");
                        flag_1 = true;
                        log_context.Text += "[" + TM.Text + "]" + "SAMPPLING: 1/4.\r\n";
                    }

                    LabelTimer.IsEnabled = true;
                    ShowTimer.Stop();
                    LabelTimer.Start();
                }
                else if (Exp_State == 1)
                {
                    Exp_State = 0;
                    melsecSerial.Write("M500", false);
                    melsecSerial.Write("M501", false);
                    melsecSerial.Write("M502", false);
                    melsecSerial.Write("M503", false);
                    melsecSerial.Write("M504", false);
                    LabelTimer.Stop();
                    ShowTimer.Start();

                    this.START.Content = "START";
                    this.START.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                    log_context.Text += "[" + TM.Text + "]" + " Labelling ABORT.\r\n";
                }
            }
            else
            {
                MessageBox.Show("Connect device first!");
            }
        }

        private void COM_REFRESH_Click(object sender, RoutedEventArgs e)
        {
            COM_LIST.Items.Clear();
            string[] ArrayComPortsName = SerialPort.GetPortNames(); //获取当前串口个数名称
            if (ArrayComPortsName.Length != 0)
            {
                Array.Sort(ArrayComPortsName);
                for (int i = 0; i < ArrayComPortsName.Length; i++)
                {
                    COM_LIST.Items.Add(ArrayComPortsName[i]);
                }
                if (ArrayComPortsName.Length == 1)
                {
                    COM_LIST.SelectedIndex = 0;
                }
            }
        }

        private void COM_CONNECT_Click(object sender, RoutedEventArgs e)
        {
            if (!COM_Connectted & Exp_State == 0)
            {
                //melsecSerial?.Close();
                melsecSerial = new MelsecFxSerial();
                try
                {
                    melsecSerial.SerialPortInni(sp =>
                    {
                        sp.PortName = COM_LIST.Items[COM_LIST.SelectedIndex].ToString();
                        sp.BaudRate = 19200;
                        sp.DataBits = 7;
                        sp.StopBits = (StopBits)1;
                        sp.Parity = System.IO.Ports.Parity.Even; //System.IO.Ports.Parity.Even
                    });
                    melsecSerial.Open();
                    melsecSerial.Write("M500", false);
                    melsecSerial.Write("M501", false);
                    melsecSerial.Write("M502", false);
                    melsecSerial.Write("M503", false);
                    melsecSerial.Write("M504", false);
                    if (melsecSerial.ReadUInt16("D200").IsSuccess)
                    {
                        LED_Volt.Text = (melsecSerial.ReadUInt16("D200").Content / 40).ToString();
                    }
                    MFC_N2_SET.Text = MFC_CO2_SET.Text = "0";
                    RefreshThread = new Thread(RefreshData);
                    RefreshThread.Start();
                    COM_Connectted = true;
                    COM_CONNECT.Content = "DISCONNECT";
                    LED_Volt.IsEnabled = MFC_N2_SET.IsEnabled = MFC_CO2_SET.IsEnabled = true;
                    log_context.Text += "[" + TM.Text + "]" + COM_LIST.Items[COM_LIST.SelectedIndex].ToString() + " connected.\r\n";
                }
                catch 
                {
                    MessageBox.Show("Can not detect the device.");
                }

            }
            else if (COM_Connectted & Exp_State == 0)
            {
                RefreshThread.Abort();
                try
                {
                    if (melsecSerial.ReadBool("M500").IsSuccess)
                    {
                        melsecSerial.Write("M500", false);
                    }
                    else
                    {
                        log_context.Text += "[" + TM.Text + "] CO2 valve not responsed.\r\n";
                    }
                    melsecSerial.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                COM_Connectted = false;
                COM_CONNECT.Content = "CONNECT";
                LED_Volt.IsEnabled = MFC_N2_SET.IsEnabled = MFC_CO2_SET.IsEnabled = false;
                log_context.Text += "[" + TM.Text + "]" + COM_LIST.Items[COM_LIST.SelectedIndex].ToString() + " is disconnected.\r\n";
            }
            else if (!COM_Connectted & Exp_State == 1)
            {
                MessageBox.Show("Experiment is in progress.");
            }
        }

        public static float ObjectToFloat(object obj2Float)
        {
            float result = 0.00f;   //默认值
            if (obj2Float != null)
            {
                string str2Float = obj2Float.ToString();        //object to string
                if (!float.TryParse(str2Float, out result))     //string直接转换为float,若失败，则获取字符串前部分数字转换为float
                {
                    string strNumber = string.Empty;
                    foreach (char iChr in str2Float)
                    {
                        if (Char.IsNumber(iChr))
                        {
                            strNumber += iChr;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(strNumber))
                    {
                        float.TryParse(strNumber, out result);
                    }
                }
            }
            return result;
        }

        private void Samplling(MelsecFxSerial melsecSerial,String register)
        {

            t = new Thread(() =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                melsecSerial.Write(register,true);
                while (stopWatch.ElapsedMilliseconds < 5000)
                {
                    int timeout = 5000 - (int)stopWatch.ElapsedMilliseconds;
                    Thread.Sleep(timeout >= 0 ? timeout : 0);
                }
                melsecSerial.Write(register, false);
                stopWatch.Stop();
            });    
            t.IsBackground = true;
            t.Start();
        }

        private void Sampling_Interval_0_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sample_1 = Convert.ToInt32(SAMPLE_1.Text);
                sample_2 = Convert.ToInt32(SAMPLE_2.Text);
                sample_3 = Convert.ToInt32(SAMPLE_3.Text);
                sample_4 = Convert.ToInt32(SAMPLE_4.Text);
                string interval = sample_1.ToString() + ", " + sample_2.ToString() + ", " + sample_3.ToString() + ", " + sample_4.ToString();
                log_context.Text += "[" + TM.Text + "]" + "Sampling interval is set to: " + interval + "s.\r\n";
            }
        }

        private void Sampling_Interval_1_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sample_1 = Convert.ToInt32(SAMPLE_1.Text);
                sample_2 = Convert.ToInt32(SAMPLE_2.Text);
                sample_3 = Convert.ToInt32(SAMPLE_3.Text);
                sample_4 = Convert.ToInt32(SAMPLE_4.Text);
                string interval = sample_1.ToString() + ", " + sample_2.ToString() + ", " + sample_3.ToString() + ", " + sample_4.ToString();
                log_context.Text += "[" + TM.Text + "]" + "Sampling interval is set to: " + interval + "s.\r\n";
                
            }
        }

        private void Sampling_Interval_2_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sample_1 = Convert.ToInt32(SAMPLE_1.Text);
                sample_2 = Convert.ToInt32(SAMPLE_2.Text);
                sample_3 = Convert.ToInt32(SAMPLE_3.Text);
                sample_4 = Convert.ToInt32(SAMPLE_4.Text);
                string interval = sample_1.ToString() + ", " + sample_2.ToString() + ", " + sample_3.ToString() + ", " + sample_4.ToString();
                log_context.Text += "[" + TM.Text + "]" + "Sampling interval is set to: " + interval + "s.\r\n";
                
            }

        }

        private void Sampling_Interval_3_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sample_1 = Convert.ToInt32(SAMPLE_1.Text);
                sample_2 = Convert.ToInt32(SAMPLE_2.Text);
                sample_3 = Convert.ToInt32(SAMPLE_3.Text);
                sample_4 = Convert.ToInt32(SAMPLE_4.Text);
                string interval = sample_1.ToString() + ", " + sample_2.ToString() + ", " + sample_3.ToString() + ", " + sample_4.ToString();
                log_context.Text += "[" + TM.Text + "]" + "Sampling interval is set to: " + interval + "s.\r\n";
                
            }
        }

        private void PRE_CHECK_Checked(object sender, RoutedEventArgs e)
        {
            PRE_LABEL.IsEnabled = true;
            pre_label_time = Convert.ToInt32(PRE_LABEL.Text);
        }

        private void PRE_CHECK_Unchecked(object sender, RoutedEventArgs e)
        {
            PRE_LABEL.IsEnabled = false;
            pre_label_time = 0;
        }

        private void LOG_TextChanged(object sender, TextChangedEventArgs e)
        {
            LOG.ScrollToEnd();
        }

        private void U1_SW_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M501", true);
            }
        }

        private void U1_SW_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M501", false);
            }
        }

        private void U2_SW_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M502", true);
            }
        }

        private void U2_SW_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M502", false);
            }
        }

        private void U3_SW_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M503", true);
            }
        }

        private void U3_SW_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M503", false);
            }
        }

        private void U4_SW_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M504", true);
            }
        }

        private void U4_SW_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M504", false);
            }
        }

        private void U1_SW_TouchDown(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M501", true);
            }
        }

        private void U1_SW_TouchUp(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M501", false);
            }
        }

        private void U2_SW_TouchDown(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M502", true);
            }
        }

        private void U2_SW_TouchUp(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M502", false);
            }
        }

        private void U3_SW_TouchDown(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M503", true);
            }
        }

        private void U3_SW_TouchUp(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M503", false);
            }
        }

        private void U4_SW_TouchDown(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M504", true);
            }
        }

        private void U4_SW_TouchUp(object sender, TouchEventArgs e)
        {
            if (COM_Connectted)
            {
                melsecSerial.Write("M504", false);
            }
        }

        private void LABEL_SW_Click(object sender, RoutedEventArgs e)
        {
            if (COM_Connectted)
            {
                bool status = melsecSerial.ReadBool("M500").Content;
                melsecSerial.Write("M500", !status);
                LABEL_SW.IsChecked = !status;
                if (!status)
                    log_context.Text += "[" + TM.Text + "]" + "Label switch is turned ON manually.\r\n";
                else
                    log_context.Text += "[" + TM.Text + "]" + "Label switch is turned OFF manually.\r\n";
            }
            else
                LABEL_SW.IsChecked = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (RefreshThread != null)
            {
                if (RefreshThread.IsAlive)
                    RefreshThread.Abort();
            }
            if (Exp_State == 0)
            {
                SetThreadExecutionState(ES_CONTINUOUS);
                Application.Current.Shutdown();
            }
            else if (Exp_State == 1)
            {
                MessageBoxResult dr = MessageBox.Show("Experiment is in progress. Sure to exit?", "WARNNING", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (dr == MessageBoxResult.OK)
                {
                    SetThreadExecutionState(ES_CONTINUOUS);
                    Application.Current.Shutdown();
                }
            }
        }
    }
}

/*
0-4000<->0-5V returned value
AD0 D8030 D100 MFC_N2
AD1 D8031 D101 MFC_CO2
AD2 D8032 D102 RH
AD3 D8033 D103 MFM
AD4 D8034 D104 
AD5 D8035 D105

0-4000<->0-5V signal
DA0 D8050 D200 LED0
DA1 D8051 D201 MFC_N2
DA2 D8052 D202 MFC_CO2
DA3 D8053 D203

Y000 M500 CO2_SW
Y001 M501 ISFU_0
Y002 M502 ISFU_1
Y003 M503 ISFU_2
Y004 M504 ISFU_3
Y005
Y006
Y007
*/
