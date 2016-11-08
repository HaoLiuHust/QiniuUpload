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
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;

namespace QiniuUpload
{
    /// <summary>
    /// SetAccount.xaml 的交互逻辑
    /// </summary>
    public partial class SetAccountWindow : Window
    {
        public SetAccountWindow()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.UserAccount.SpaceName = SpaceName.Text;
            MainWindow.UserAccount.SecretKey = SecretKey.Text;
            MainWindow.UserAccount.AccessKey = AccessKey.Text;
            MainWindow.UserAccount.HostName = HostName.Text;
            using (Stream s = File.OpenWrite(Properties.Resources.ConfigFilePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AccountInfo));
                xs.Serialize(s, MainWindow.UserAccount);
            }
            this.Close();          
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
