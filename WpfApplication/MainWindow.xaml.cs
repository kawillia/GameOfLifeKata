﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.ComponentModel;

namespace GameOfLife.WpfApplication
{
    public partial class MainWindow : Window
    {
        private const Int32 NumberOfRows = 30;
        private const Int32 NumberOfColumns = 30;
        private BackgroundWorker backgroundWorker;
        private Boolean restart = false;

        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();
        }

        protected void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (restart)
            {
                backgroundWorker.RunWorkerAsync();
                restart = false;
            }
        }

        protected void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var cells = e.UserState as Boolean[,];
            DisplayCells(cells);
        }

        protected void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var grid = new GameOfLife.Core.Grid(NumberOfRows, NumberOfColumns);
            RandomlySeedGrid(grid);

            while (true)
            {
                if (backgroundWorker.CancellationPending)
                    break;

                grid.Update();

                var cells = grid.GetCells();
                backgroundWorker.ReportProgress(0, cells);
                Thread.Sleep(300);
            }
        }

        private static void RandomlySeedGrid(GameOfLife.Core.Grid grid)
        {
            var numberOfCells = grid.NumberOfRows * grid.NumberOfColumns;
            var random = new Random();

            for (var i = 0; i < numberOfCells; i++)
            {
                var randomRowNumber = random.Next(1, grid.NumberOfRows);
                var randomColumnNumber = random.Next(1, grid.NumberOfColumns);

                grid.BringToLife(randomRowNumber, randomColumnNumber);
            }
        }

        private void DisplayCells(Boolean[,] cells)
        {
            dynamicGrid.Children.Clear();
            dynamicGrid.ColumnDefinitions.Clear();
            dynamicGrid.RowDefinitions.Clear();

            var rowHeight = (Double)dynamicGrid.Height / (Double)NumberOfRows;
            var columnHeight = (Double)dynamicGrid.Width / (Double)NumberOfColumns;

            for (var i = 0; i < NumberOfRows; i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(rowHeight);
                dynamicGrid.RowDefinitions.Add(row);

                for (var j = 0; j < NumberOfColumns; j++)
                {
                    var column = new ColumnDefinition();
                    column.Width = new GridLength(columnHeight);
                    dynamicGrid.ColumnDefinitions.Add(column);                    

                    var panel = new DockPanel();

                    if (cells[i, j])
                        panel.Background = new SolidColorBrush(Colors.Black);
                    else
                        panel.Background = new SolidColorBrush(Colors.White);

                    Grid.SetRow(panel, i);
                    Grid.SetColumn(panel, j);
                    
                    dynamicGrid.Children.Add(panel);
                }
            }
        }

        private void replayButton_Click(Object sender, RoutedEventArgs e)
        {
            backgroundWorker.CancelAsync();
            restart = true;
        }
    }
}
