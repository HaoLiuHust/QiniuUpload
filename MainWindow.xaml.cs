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
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace QiniuUpload
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static AccountInfo UserAccount = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserAccount = new AccountInfo();
            if (File.Exists(Properties.Resources.ConfigFilePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AccountInfo));
                using (Stream s = File.OpenRead(Properties.Resources.ConfigFilePath))
                {
                    UserAccount = (AccountInfo)(xs.Deserialize(s));
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

                }
            }           
        }
    }
}
