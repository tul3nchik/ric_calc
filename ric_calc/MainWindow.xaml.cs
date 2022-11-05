using Ghostscript.NET;
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
        GhostscriptPageInkCoverage pic;
        Dictionary<int, GhostscriptPageInkCoverage> pages;

        public int bwPages = 0;
        public int color15Pages = 0;
        public int colorUnder45Pages = 0;
        public int coloroOver45Pages = 0;
        public int color90Pages = 0;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            //Функция открытия файла
            chsFileDialog.Filter = "Файл PDF (*.pdf)|*.pdf";
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
            calcOutput.Text = "";
            calcFile.IsEnabled = false;
            calcFile.Content = "Загрузка...";
            executeCaclBook();
        }

        async public void executeCaclBook()
        {
            await Task.Run(() => calcBookCost());
        }

        public void calcBookCost()
        {
            //Функция подсчёта книги
            string inputFile = @filename;
            pages = GhostscriptPdfInfo.GetInkCoverage(inputFile);

            var C = pages[pic.Page].C;
            var M = pages[pic.Page].M;
            var Y = pages[pic.Page].Y;
            var K = pages[pic.Page].K;

            foreach (KeyValuePair<int, GhostscriptPageInkCoverage> kvp in pages)
            {
                pic = kvp.Value;
                Dispatcher.Invoke(() => calcOutput.Text += "\nСтраница: " + pic.Page + "\nCyan: " + pages[pic.Page].C + " Magenta: " + pages[pic.Page].M + "\nYellow: " + pages[pic.Page].Y + " Key (black): " + pages[pic.Page].K + "\n");

                if (C == M && M == Y && K == Y) bwPages += 1;
                else
                {
                    bwPages += C == 0.0 && M == 0.0 && Y == 0.0 ? 1 : 0;
                    color15Pages += C >= 0.1 && M >= 0.1 && Y >= 0.1 && C <= 15.0 && M <= 15.0 && Y <= 15.0 ? 1 : 0;
                    colorUnder45Pages += C >= 15.1 && M >= 15.1 && Y >= 15.1 && C <= 45.0 && M <= 45.0 && Y <= 45.0 ? 1 : 0;
                    coloroOver45Pages += C >= 45.1 && M >= 45.1 && Y >= 45.1 && C <= 89.9 && M <= 89.9 && Y <= 89.9 ? 1 : 0;
                    color90Pages += C >= 90.0 && M >= 90.0 && Y >= 90.0 && C <= 100.0 && M <= 100.0 && Y <= 100.0 ? 1 : 0;
                }
            }
            Dispatcher.Invoke(() => calcOutput.Text += "\nЧБ: " + bwPages + " 15%: " +
                color15Pages + "\nдо 45%: " + colorUnder45Pages + " больше 45%: " +
                coloroOver45Pages + " 90% заливки: " + color90Pages + "\nCтраниц всего: " + pages.Count);
            Dispatcher.Invoke(() => calcFile.IsEnabled = true);
            Dispatcher.Invoke(() => calcFile.Content = "Рассчитать");
            bwPages = color15Pages = colorUnder45Pages = coloroOver45Pages = color90Pages = 0;
        }
    }
}