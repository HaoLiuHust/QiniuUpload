using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading;
using System.IO;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Windows.Forms;
using Qiniu.Util;
using Qiniu.Storage;
using Qiniu.Http;
using System.ComponentModel;
using System.Diagnostics;

namespace QiniuUpload
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static AccountInfo UserAccount = null;
        public static PutPolicy putPolicy = null;
        public string URL = null;
        private HwndSource hWndSource = null;
        private bool IsViewing = false;
        private string _messagestr = Properties.Resources.NoUrlMessage;
        private Dictionary<string, List<string>> historyLog;

        public event PropertyChangedEventHandler PropertyChanged;

        public String MessageStr
        {
            get { return _messagestr;}
            set
            {
                _messagestr = value;
                if(this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MessageStr"));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            System.Windows.Controls.TextBlock tb = new System.Windows.Controls.TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tb.FontSize = 20;
            tb.Text = "复制、截图或点击此处选择图片上传";
            this.UpLoadZone.Children.Add(tb);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsViewing)
                StopCBViewer();

            WriteHistoryLog();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserAccount = new AccountInfo();
            putPolicy = new PutPolicy();
            putPolicy.Scope = UserAccount.SpaceName;
            putPolicy.SetExpires(3600*24*30);
            putPolicy.DeleteAfterDays = 365;
            if (File.Exists(Properties.Resources.ConfigFilePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AccountInfo));
                using (Stream s = File.OpenRead(Properties.Resources.ConfigFilePath))
                {
                    UserAccount = (AccountInfo)(xs.Deserialize(s));
                }
            }

            if(!Directory.Exists(Properties.Resources.ImageSavePathDir))
            {
                Directory.CreateDirectory(Properties.Resources.ImageSavePathDir);
            }

            System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("MessageStr");
            BindingOperations.SetBinding(this.MessageText, TextBlock.TextProperty, binding);
            historyLog = new Dictionary<string, List<string>>();
            LoadHistory(Properties.Resources.HistoryLog, ref historyLog);
        }

        private void LoadHistory(string logfile, ref Dictionary<string, List<string> > historylog)
        {
            if(File.Exists(logfile))
            {
                if(historylog == null)
                {
                    historylog = new Dictionary<string, List<string>>();
                }
                historylog.Clear();

                using (StreamReader fs = new StreamReader(logfile, Encoding.Default))
                {
                    string line = fs.ReadLine();                  
                    string[] linesplit = line.Split();
                    if(linesplit.Length >= 2)
                    {
                        string hashkey = linesplit[0];
                        if(!historylog.ContainsKey(hashkey))
                        {
                            historylog[hashkey] = new List<string>(linesplit);
                            historylog[hashkey].RemoveAt(0);
                        }
                        
                    }
                }
            }
        }

        private void WriteHistoryLog()
        {
            if(historyLog.Count > 0)
            {
                using (StreamWriter sw = new StreamWriter(Properties.Resources.HistoryLog, false))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach(var pair in historyLog)
                    {
                        sb.Clear();
                        sb.Append(pair.Key);
                        sb.Append(" ");
                        sb.Append(string.Join(" ", pair.Value.ToArray()));
                        sw.WriteLine(sb);
                    }
                }
            }
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            SetAccountWindow SetAccountWindow = new SetAccountWindow();
            if (File.Exists(Properties.Resources.ConfigFilePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AccountInfo));
                using (Stream s = File.OpenRead(Properties.Resources.ConfigFilePath))
                {
                    UserAccount = (AccountInfo)(xs.Deserialize(s));
                    SetAccountWindow.SpaceName.Text = UserAccount.SpaceName;
                    SetAccountWindow.AccessKey.Text = UserAccount.AccessKey;
                    SetAccountWindow.SecretKey.Text = UserAccount.SecretKey;
                    SetAccountWindow.HostName.Text = UserAccount.HostName;
                }
            }           
            SetAccountWindow.ShowDialog();
        }

        private void UpLoadZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using (OpenFileDialog FileBrowser = new OpenFileDialog())
            {
                FileBrowser.Filter = "picture|*.jpg;*.png;*.bmp";
                FileBrowser.RestoreDirectory = true;

                if(FileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MessageStr = Properties.Resources.UpLoading;
                    UpLoadFile(FileBrowser.FileName);
                    MessageStr = string.IsNullOrEmpty(URL)?MessageStr:URL;
                }
            }           
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(URL))
            {
                MessageStr = Properties.Resources.NoUrlMessage;
            }
            else
            {
                System.Windows.Clipboard.SetText(URL);
                MessageStr = Properties.Resources.ResultMessage;
            }
        }

        private void InitCBViewer()
        {
            WindowInteropHelper lwindowih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(lwindowih.Handle);

            hWndSource.AddHook(this.WndProc);
            Win32.AddClipboardFormatListener(hWndSource.Handle);
            IsViewing = true;
        }

        private void StopCBViewer()
        {
            Win32.RemoveClipboardFormatListener(hWndSource.Handle);
            hWndSource.RemoveHook(this.WndProc);
            IsViewing = false;
        }

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);
        //    HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
        //    source.AddHook(WndProc);
        //}

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParm, ref bool handled)
        {
            if(msg == Win32.WM_CLIPBOARDUPDATE)
            {
                ProcessClipBoard();
            }
            return IntPtr.Zero;
        }

        private void CBViewerButton_Click(object sender, RoutedEventArgs e)
        {
            if(!IsViewing)
            {
                InitCBViewer();
                this.CBViewerButton.Content = "停止监控";
            }
            else
            {
                StopCBViewer();
                this.CBViewerButton.Content = "监控剪切板";
            }
        }

        private void ProcessClipBoard()
        {
            if(System.Windows.Clipboard.ContainsImage())
            {
                MessageStr = Properties.Resources.UpLoading;

                BmpBitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(System.Windows.Clipboard.GetImage()));

                string lFileName = CreateFileName();
                string lSaveFilePath = System.IO.Path.Combine(Properties.Resources.ImageSavePathDir, lFileName);
                using (FileStream fs = new FileStream(lSaveFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    enc.Save(fs);
                    fs.Close();
                }

                //because unkown reason, when use wechat snapshot hotkey, the message will process twice, to avoid this, check whether we save the same file
                string lLocalHash = QETag.hash(lSaveFilePath);
                if(historyLog.ContainsKey(lLocalHash))
                {
                    File.Delete(lSaveFilePath);
                    URL = CreateURL(historyLog[lLocalHash][0]);
                    lSaveFilePath = System.IO.Path.Combine(Properties.Resources.ImageSavePathDir, historyLog[lLocalHash][0]);                   
                }
                else
                {                   
                    if(!UpLoadFile(lSaveFilePath))
                    {
                        File.Delete(lSaveFilePath);
                    }
                }                
            }
        }

        private bool UpLoadFile(string filepath)
        {
            string filename = System.IO.Path.GetFileName(filepath);
            Qiniu.Util.Mac lMac = new Qiniu.Util.Mac(UserAccount.AccessKey, UserAccount.SecretKey);
            string lRemoteHash = RemoteFileInfo.RemoteFileStat(lMac, UserAccount.SpaceName, filename);
            bool lSkip = false;
            bool lUpLoadSuccess = false;
            //check local
            string lLocalHash = String.Empty;
            if (!string.IsNullOrEmpty(lRemoteHash))
            {
                lLocalHash = QETag.hash(filepath);

                if (historyLog.ContainsKey(lLocalHash))
                {
                    if(historyLog[lLocalHash].Contains(filename))
                    {
                        lSkip = true;
                        URL = CreateURL(filename);
                        lUpLoadSuccess = true;
                    }                   
                }
                else if (string.Equals(lLocalHash, lRemoteHash))
                {
                    lSkip = true;
                    URL = CreateURL(filename);
                    lUpLoadSuccess = true;
                }
            }
            if (!lSkip)
            {
                string lUploadToken = Auth.createUploadToken(putPolicy, lMac);
                UploadManager lUploadMgr = new UploadManager();
                lUploadMgr.uploadFile(filepath, filename, lUploadToken, null, new UpCompletionHandler(delegate (string key, ResponseInfo responseinfo, string response)
                {
                    if (responseinfo.StatusCode != 200)
                    {
                        MessageStr = Properties.Resources.ErrorMessage;
                    }
                    else
                    {
                        MessageStr = Properties.Resources.SuccessMessage;
                        if (historyLog.ContainsKey(lLocalHash))
                        {
                            historyLog[lLocalHash].Add(filename);
                        }
                        URL = CreateURL(filename);
                        lUpLoadSuccess = true;
                    }
                }));                
            }

            if(lUpLoadSuccess)
                DisplayImage(filepath);

            return lUpLoadSuccess;
        }

        private void DisplayImage(string filepath)
        {
            if(File.Exists(filepath))
            {
                this.UpLoadZone.Children.Clear();
                Image img = new Image() { Width = this.UpLoadZone.Width, Height = this.UpLoadZone.Height, Stretch = Stretch.UniformToFill };

                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    BmpBitmapDecoder bd = new BmpBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    img.Source = bd.Frames[0];
                    this.UpLoadZone.Children.Add(img);
                }               
            }
            
        }

        private string CreateFileName()
        {
            DateTime lNowTime = DateTime.Now;
            Random rd = new Random();
            int ranNum = rd.Next(1, 1000);
            string UniqueFileName = lNowTime.ToString("yyyyMMddHHmmss")+ranNum.ToString()+".bmp";
            return UniqueFileName;
        }

        private string CreateURL(string filename)
        {
            return System.IO.Path.Combine(UserAccount.HostName, filename);
        }
    }
}
