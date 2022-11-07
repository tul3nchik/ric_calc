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
        private readonly OpenFileDialog _selectFileDialog = new OpenFileDialog();
        private GhostscriptPageInkCoverage _gsPageInkCoverage;
        private Dictionary<int, GhostscriptPageInkCoverage> _gsPages;
        private SQLiteConnection _sqliteCon;
        private SQLiteCommand _sqliteCommand;

        private string _filename;
        private int _bwPages;
        private int _color15Pages;
        private int _colorUnder45Pages;
        private int _colorOver45Pages;
        private int _color90Pages;
        private int _bwpc;
        private int _c15C;
        private int _cu45C;
        private int _co45C;
        private int _c90C;
        private PaperType _paperType = PaperType.None;
        private double _price;
        private double _totalPrice;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            //Функция открытия файла
            _selectFileDialog.Filter = "Файл PDF (*.pdf)|*.pdf";
            _selectFileDialog.FilterIndex = 2;
            _selectFileDialog.RestoreDirectory = true;

            if (_selectFileDialog.ShowDialog() == true)
            {
                _filename = _selectFileDialog.FileName;
                SelectFileLabel.Content = _filename;
                CalcFile.IsEnabled = true;
            }
        }

        private void calcFile_Click(object sender, RoutedEventArgs e)
        {
            CalcOutput.Text = "";
            CalcFile.IsEnabled = false;
            CalcFile.Content = "Загрузка...";
            ExecuteCalcBook();
        }

        private async void ExecuteCalcBook()
        {
            await Task.Run(CalculateBookCoverage);

            //проверка на выбранный тип бумаги
            if (_paperType != PaperType.None)
            {
                _totalPrice = CalculatePrice();
                MessageBox.Show($"Итоговая цена: {_totalPrice}руб.");
            }
            else
            {
                _totalPrice = 0.0;
            }

            //блок вывода информации
            CalcOutput.Text += $"\nЧБ: {_bwPages}\n" +
                               $"15%: {_color15Pages}\n" +
                               $"до 45%: {_colorUnder45Pages}\n" +
                               $"больше 45%: {_colorOver45Pages}\n" +
                               $"90% заливки: {_color90Pages}\n" +
                               $"Страниц всего: {_gsPages.Count}";
            
            CalcFile.IsEnabled = true;
            CalcFile.Content = "Рассчитать";

            _bwPages = _color15Pages = _colorUnder45Pages = _colorOver45Pages = _color90Pages = 0;
        }

        private double CalculatePrice()
        {
            //блок подключения к бд, и вытяжки из неё данных
            _sqliteCon = new SQLiteConnection(@"Data Source = paperPrice.db");
            _sqliteCon.Open();
            _sqliteCommand = _sqliteCon.CreateCommand();
            _sqliteCommand.CommandText = $"SELECT priceBW, price15, priceU45, priceO45, price90 FROM price_list WHERE paperType = '{_paperType.ToString()}';";
            try
            {
                var rdr = _sqliteCommand.ExecuteReader();
                while (rdr.Read())
                {
                    _bwpc = Convert.ToInt32(rdr[0]);
                    _c15C = Convert.ToInt32(rdr[1]);
                    _cu45C = Convert.ToInt32(rdr[2]);
                    _co45C = Convert.ToInt32(rdr[3]);
                    _c90C = Convert.ToInt32(rdr[4]);
                }
                rdr.Close();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.HelpLink}\n{ex.Data}");
            }
            //блок подсчёта
            _sqliteCon.Close();
            _price = (_bwPages * _bwpc) +
                (_color15Pages * _c15C) +
                (_colorUnder45Pages * _cu45C) +
                (_colorOver45Pages * _co45C) +
                (_color90Pages * _c90C);
            //строка с возвращением итоговой цены
            return _price;
        }

        private void CalculateBookCoverage()
        {
            //блок расчёта цветных и ч/б страниц
            var inputFile = _filename;
            _gsPages = GhostscriptPdfInfo.GetInkCoverage(inputFile);
            var coveragePercentage = 0.0;

            //цикл переборки чб страниц и цветных (с последующей сортировкой по проценту заливки)
            foreach (var kvp in _gsPages)
            {
                _gsPageInkCoverage = kvp.Value;

                var C = _gsPages[_gsPageInkCoverage.Page].C;
                var M = _gsPages[_gsPageInkCoverage.Page].M;
                var Y = _gsPages[_gsPageInkCoverage.Page].Y;
                var K = _gsPages[_gsPageInkCoverage.Page].K;
                if (C == M && M == Y && Y == K) _bwPages += 1;
                else if (C == 0.0 && M == 0.0 && Y == 0.0) _bwPages += 1;
                else
                {
                    coveragePercentage = (C + M + Y) / 3;
                    if (coveragePercentage <= 15.0) _color15Pages += 1;
                    if (coveragePercentage > 15.0 && coveragePercentage <= 45.0) _colorUnder45Pages += 1;
                    if (coveragePercentage > 45.0 && coveragePercentage <= 90.0) _colorOver45Pages += 1;
                    if (coveragePercentage > 90.0) _color90Pages += 1;
                }
                //вывод постраничного отчёта
                CalcOutput.Text += $"Страница: {_gsPageInkCoverage.Page} Заливка: {coveragePercentage}%\n" +
                                   $"C: {C} M: {M} Y: {Y} K: {K}\n";
            }
        }

        private void aFourCB_Checked(object sender, RoutedEventArgs e)
        {
            AFiveCheckBox.IsEnabled = false;
            _paperType = PaperType.A4;
        }

        private void aFiveCB_Unchecked(object sender, RoutedEventArgs e)
        {
            AFourCheckBox.IsEnabled = true;
            _paperType = PaperType.None;
        }

        private void aFiveCB_Checked(object sender, RoutedEventArgs e)
        {
            AFourCheckBox.IsEnabled = false;
            _paperType = PaperType.A5;
        }

        private void aFourCB_Unchecked(object sender, RoutedEventArgs e)
        {
            AFiveCheckBox.IsEnabled = true;
            _paperType = PaperType.None;
        }
        
    }
}