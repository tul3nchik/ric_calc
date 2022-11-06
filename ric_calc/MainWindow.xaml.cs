using Ghostscript.NET;
using Microsoft.Win32;
using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Data;
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
    public partial class MainWindow : Window
    {
        OpenFileDialog chsFileDialog = new OpenFileDialog();
        GhostscriptPageInkCoverage pic;
        Dictionary<int, GhostscriptPageInkCoverage> pages;
        DataTable dt = new DataTable();
        SQLiteConnection sqlite;
        SQLiteCommand cmd;

        public string filename;
        public int bwPages = 0;
        public int color15Pages = 0;
        public int colorUnder45Pages = 0;
        public int colorOver45Pages = 0;
        public int color90Pages = 0;
        public int BWPC = 0;
        public int C15C = 0;
        public int CU45C = 0;
        public int CO45C = 0;
        public int C90C = 0;
        public string paperType="none";
        public double price = 0.0;
        public double totalPrice = 0;


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
            executeCalcBook();
        }

        async public void executeCalcBook()
        {
            await Task.Run(() => calcBookCost());
        }

        public double priceCalc(int pbw, int p15, int pu45, int po45, int p90, string pT)
        {
            //блок подключения к бд, и вытяжки из неё данных
            sqlite = new SQLiteConnection(@"Data Source = paperPrice.db");
            sqlite.Open();
            cmd = sqlite.CreateCommand();
            cmd.CommandText = "SELECT priceBW, price15, priceU45, priceO45, price90 FROM price_list WHERE paperType = '" + paperType + "';";
            try
            {
                SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    BWPC = Convert.ToInt32(rdr[0]);
                    C15C = Convert.ToInt32(rdr[1]);
                    CU45C = Convert.ToInt32(rdr[2]);
                    CO45C = Convert.ToInt32(rdr[3]);
                    C90C = Convert.ToInt32(rdr[4]);
                }
                rdr.Close();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(""+ex.Message+"\n"+ex.HelpLink+"\n"+ex.Data);
            }
            //блок подсчёта
            sqlite.Close();
            price = (bwPages * BWPC) +
                (color15Pages * C15C) +
                (colorUnder45Pages * CU45C) +
                (colorOver45Pages * CO45C) +
                (color90Pages * C90C);
            //строка с возвращением итоговой цены
            return price;
        }

        public void calcBookCost()
        {
            //блок расчёта цветных и ч/б страниц
            string inputFile = @filename;
            pages = GhostscriptPdfInfo.GetInkCoverage(inputFile);

            //цикл переборки чб страниц и цветных (с последующей сортировкой по проценту заливки)
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
                //вывод постраничного отчёта
                Dispatcher.Invoke(() => calcOutput.Text += "Страница: " + pic.Page + " Заливка: " + coveragePercentage + "%\nC: " + C + " M: " + M + " Y: " + Y + " K: " + K + "\n");
                coveragePercentage = 0.0;
            }
            if (paperType != "none")
            {
                totalPrice = priceCalc(bwPages, color15Pages, colorUnder45Pages, colorOver45Pages, color90Pages, paperType);
                MessageBox.Show("Итоговая цена: " + totalPrice + " руб.");
            }
            else
            {
                totalPrice = 0.0;
            }

            //блок вывода информации
            Dispatcher.Invoke(() => calcOutput.Text += "\nЧБ: " + bwPages + "\n15%: " +
                color15Pages + "\nдо 45%: " + colorUnder45Pages + "\nбольше 45%: " +
                colorOver45Pages + "\n90% заливки: " + color90Pages + "\nCтраниц всего: " + pages.Count);
            Dispatcher.Invoke(() => calcFile.IsEnabled = true);
            Dispatcher.Invoke(() => calcFile.Content = "Рассчитать");
            bwPages = color15Pages = colorUnder45Pages = colorOver45Pages = color90Pages = 0;
        }

        private void aFourCB_Checked(object sender, RoutedEventArgs e)
        {
            aFiveCB.IsEnabled = false;
            paperType = "a4";
        }

        private void aFiveCB_Unchecked(object sender, RoutedEventArgs e)
        {
            aFourCB.IsEnabled = true;
            paperType = "none";
        }

        private void aFiveCB_Checked(object sender, RoutedEventArgs e)
        {
            aFourCB.IsEnabled = false;
            paperType = "a5";
        }

        private void aFourCB_Unchecked(object sender, RoutedEventArgs e)
        {
            aFiveCB.IsEnabled = true;
            paperType = "none";
        }
    }
}