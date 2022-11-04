﻿using Ghostscript.NET;
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
            //Функция подсчёта книги
            string inputFile = @filename;
            pages = GhostscriptPdfInfo.GetInkCoverage(inputFile);

            foreach (KeyValuePair<int, GhostscriptPageInkCoverage> kvp in pages)
            {
                pic = kvp.Value;
                calcOutput.Text += "Страница: " + pic.Page + "\n Cyan: " + pages[pic.Page].C + " Magenta: " + pages[pic.Page].M + "\n Yellow: " + pages[pic.Page].Y + " Key (black): " + pages[pic.Page].K + "\n";

                bwPages += 
                    pages[pic.Page].C == 0.0 && pages[pic.Page].M == 0.0 && pages[pic.Page].Y == 0.0 
                    ? 1 : 0;

                color15Pages += 
                    pages[pic.Page].C >= 0.1 && pages[pic.Page].M >= 0.1 && pages[pic.Page].Y >= 0.1 &&
                    pages[pic.Page].C <= 15.0 && pages[pic.Page].M <= 15.0 && pages[pic.Page].Y <= 15.0 
                    ? 1 : 0;
                colorUnder45Pages +=
                    pages[pic.Page].C >= 15.1 && pages[pic.Page].M >= 15.1 && pages[pic.Page].Y >= 15.1 &&
                    pages[pic.Page].C <= 45.0 && pages[pic.Page].M <= 45.0 && pages[pic.Page].Y <= 45.0
                    ? 1 : 0;
                coloroOver45Pages +=
                    pages[pic.Page].C >= 45.1 && pages[pic.Page].M >= 45.1 && pages[pic.Page].Y >= 45.1 &&
                    pages[pic.Page].C <= 89.9 && pages[pic.Page].M <= 89.9 && pages[pic.Page].Y <= 89.9
                    ? 1 : 0;
                color90Pages +=
                    pages[pic.Page].C >= 90.0 && pages[pic.Page].M >= 90.0 && pages[pic.Page].Y >= 90.0 &&
                    pages[pic.Page].C <= 100.0 && pages[pic.Page].M <= 100.0 && pages[pic.Page].Y <= 100.0
                    ? 1 : 0;
                calcOutput.Text += "\n ЧБ: " + bwPages + " 15%: " + 
                    color15Pages + " до 45%: " + colorUnder45Pages + " больше 45%: " + 
                    coloroOver45Pages + " 90% заливки: " + color90Pages;

            }
        }
    }
}
