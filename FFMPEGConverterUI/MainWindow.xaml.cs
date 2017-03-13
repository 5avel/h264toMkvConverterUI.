using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

// Install-Package Windows7APICodePack-Shell

namespace FFMPEGConverterUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] fileArray;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void inBtnOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbInFolder.Text = dlg.SelectedPath;
                }
            }

            fileArray = Directory.GetFiles(tbInFolder.Text, "*.h264", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fileArray.Length; i++)
            {
                fileArray[i] = Path.GetFileName(fileArray[i]);
            }

            pbProgres.Maximum = fileArray.Length;

            tbConsole.Text += "В папке:'" + tbInFolder.Text + "' - " + fileArray.Count() + " файлов *.h264. \n";
        }

        private void outBtnOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbOutFolder.Text = dlg.SelectedPath;
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (fileArray == null && fileArray.Count() < 0)
            {
                System.Windows.MessageBox.Show("В папке '" + tbInFolder.Text + "' нет файлов *h264!");
                return;
            }
            if (string.IsNullOrWhiteSpace(tbOutFolder.Text))
            {
                System.Windows.MessageBox.Show("Указана не существующая папка!");
                return;
            }

            string inFolder = tbInFolder.Text;
            string outFolder = tbOutFolder.Text;
            bool isRepair = cbRepair.IsChecked.Value;

            var task = Task.Factory.StartNew(() =>
            {
                using (var h264toMKV = new Converter() { InFolderPath = inFolder, OutFolderPath = outFolder })
                {

                    DateTime allStartDT = DateTime.Now;
                    int counter = 0;
                    foreach (var item in fileArray)
                    {
                        counter++;
                        DateTime itemStartDT = DateTime.Now;
                        if (h264toMKV.StartConvert(item))
                        {
                        tbConsole.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            tbConsole.Text += item + " за " + (DateTime.Now - itemStartDT).ToString() + " \n";
                        }));
                        
                        }
                        else
                        {
                            if (isRepair)
                            {
                                h264toMKV.StartConvert(item, true);
                                tbConsole.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    tbConsole.Text += item + " Был поврежден, исправлен за " + (DateTime.Now - itemStartDT).ToString() + " \n";
                                }));
                            }
                            else
                            {
                                tbConsole.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    tbConsole.Text += item + " за " + (DateTime.Now - itemStartDT).ToString() + " ----- ПОВРЕЖДЕН!!! \n";
                                }));
                            }
                        }

                        pbProgres.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pbProgres.Value = counter;
                        }));
                    }

                tbConsole.Dispatcher.BeginInvoke((Action)(() =>
                {
                    tbConsole.Text += "Финиш за " + (DateTime.Now - allStartDT).ToString() + " \n";
                    pbProgres.Value = 0;
                }));  
            }
          });

        }
    }
}
