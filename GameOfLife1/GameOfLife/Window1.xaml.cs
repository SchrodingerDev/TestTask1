using System;
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
using System.Timers;
using System.Windows.Threading;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        const int sizeX = 25;
        const int sizeY = 25;
        SqlDataAdapter adapter;
        DataTable logTable;
        //DateTime dateStart;
        //DateTime dateEnd;
        string connectionString;
        int[,] currentGeneration = new int[sizeX,sizeY];


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            BoardGrid.Children.Clear();
            //Random rng = new Random();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    //Label1.Content += currentGeneration[i, j].ToString() + "   ";
                    Button btn1 = new Button();
                    btn1.Name = "c" + i.ToString() + "_" + j.ToString();
                    btn1.Focusable = false;
                    btn1.Background = Brushes.White;
                    BoardGrid.Children.Add(btn1);
                }
                //Label1.Content += "\r\n";
            }


        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            
            string sql = "";
            logTable = new DataTable();
            SqlConnection connection = null;
            try
            {

                
                if (start.SelectedDate.HasValue && end.SelectedDate.HasValue)
                {
                    if (start.SelectedDate.Value > end.SelectedDate.Value) throw new Exception("Дата начала заданного периода превышает дату окончания!");
                    sql = "SELECT * FROM logs where startdate > \'" + start.SelectedDate.Value.ToShortDateString() + "\' and enddate < \'" + end.SelectedDate.Value.ToShortDateString() + "\'";
                }
                else if (start.SelectedDate.HasValue)
                {
                    sql = "SELECT * FROM logs where startdate > \'" + start.SelectedDate.Value.ToShortDateString() + "\'";
                }
                else if (end.SelectedDate.HasValue)
                {
                    sql = "SELECT * FROM logs where enddate < \'" + end.SelectedDate.Value.ToShortDateString() + "\'";
                }
                else
                {
                    throw new Exception("Заполните диапазон дат!");
                }
                connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter(command);

                connection.Open();
                adapter.Fill(logTable);
                logGrid.ItemsSource = logTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        void ShowStartStateBoard(object sender, RoutedEventArgs e)
        {
            int l = logGrid.SelectedIndex;
            showBoard(l, 1);
        }

        void ShowEndStateBoard(object sender, RoutedEventArgs e)
        {
            int l = logGrid.SelectedIndex;
            showBoard(l, 2);
        }

        void showBoard(int row, int col)
        {
            string board = (string)logTable.DefaultView[row].Row.ItemArray[col];
            {
                //string board = "";
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        currentGeneration[i, j] = (int)Char.GetNumericValue(board, 0);
                        board = board.Substring(1);
                    }
                }

            }
            Redraw();
            
        }

        void Redraw()
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {

                    string el = "c" + i.ToString() + "_" + j.ToString();
                    Button btn1 = (Button)LogicalTreeHelper.FindLogicalNode(BoardGrid, el);
                    if (currentGeneration[i, j] == 1)
                    {
                        btn1.Background = Brushes.Black;

                    }
                    else
                    {
                        btn1.Background = Brushes.White;
                    }
                }

            }
        }




    }
}
