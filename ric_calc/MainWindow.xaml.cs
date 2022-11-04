using Microsoft.Win32;
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

namespace ric_calc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string filename;
        OpenFileDialog chsFileDialog = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            chsFileDialog.Filter = "Файл PDF (*.pdf)|*.pdf|Все файлы (*.*)|*.*";
            chsFileDialog.FilterIndex = 2;
            chsFileDialog.RestoreDirectory = true;

            if (chsFileDialog.ShowDialog() == true)
            {
                filename = chsFileDialog.FileName;
                chsFileLabel.Content = filename;
                calcFile.IsEnabled = true;
            }
        }

        private void calcFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
