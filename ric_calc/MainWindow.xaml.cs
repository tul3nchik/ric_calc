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
        public int colorOver45Pages = 0;
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

            foreach (KeyValuePair<int, GhostscriptPageInkCoverage> kvp in pages)
            {
                pic = kvp.Value;

                var C = pages[pic.Page].C;
                var M = pages[pic.Page].M;
                var Y = pages[pic.Page].Y;
                var K = pages[pic.Page].K;
                var coveragePercentage = 0.0;
                if (C == M && M == Y && K == Y) bwPages += 1;
                else if (C == 0.0 && M == 0.0 && Y == 0.0) bwPages += 1;
                else
                {
                    coveragePercentage = (C + M + Y) / 3;
                    if (coveragePercentage <= 15.0) color15Pages += 1;
                    if (coveragePercentage > 15.0 && coveragePercentage <= 45.0) colorUnder45Pages += 1;
                    if (coveragePercentage > 45.0 && coveragePercentage <= 90.0) colorOver45Pages += 1;
                    if (coveragePercentage > 90.0) color90Pages += 1;
                }
                Dispatcher.Invoke(() => calcOutput.Text += "\nСтраница: " + pic.Page + " Заливка: " + coveragePercentage + "%\nC: " + C + " M: " + M + " Y: " + Y + " K: " + K + "\n");
                coveragePercentage = 0.0;
            }
            Dispatcher.Invoke(() => calcOutput.Text += "\nЧБ: " + bwPages + " 15%: " +
                color15Pages + "\nдо 45%: " + colorUnder45Pages + " больше 45%: " +
                colorOver45Pages + " 90% заливки: " + color90Pages + "\nCтраниц всего: " + pages.Count);
            Dispatcher.Invoke(() => calcFile.IsEnabled = true);
            Dispatcher.Invoke(() => calcFile.Content = "Рассчитать");
            bwPages = color15Pages = colorUnder45Pages = colorOver45Pages = color90Pages = 0;
        }
    }
}