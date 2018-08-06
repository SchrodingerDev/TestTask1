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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        const int sizeX = 25;
        const int sizeY = 25;
        int[,] currentGeneration = new int[sizeX, sizeY];
        int[,] nextGeneration = new int[sizeX, sizeY];
        int[,] stateGeneration = new int[sizeX, sizeY];
        int genCount = 0;
        DispatcherTimer timer = new DispatcherTimer();
        string connectionString;
        SqlDataAdapter adapter;
        SqlDataAdapter adapter2;
        SqlDataAdapter adapter3;
        DataTable savesTable;
        DataTable statesTable;
        DateTime dateStart;

        DataTable logTable;


        void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Запуск/Пауза игры

            if ((string)startButton.Content == "Старт")
            {
                foreach (UIElement el in BoardGrid.Children)
                {
                    Button btn1 = (Button)el;
                    btn1.Click -= Change;
                }
                startButton.Content = "Пауза";
                randomizeButton.IsEnabled = false;
                randomState.IsEnabled = false;
                newGameButton.IsEnabled = false;
                LoadStateButton.IsEnabled = false;
                LoadGameButton.IsEnabled = false;
                saveStateButton.IsEnabled = false;
                saveGame.IsEnabled = false;
                deleteButton.IsEnabled = false;
                deleteStateButton.IsEnabled = false;
                dateStart = DateTime.Now;
                timer.Start();

                Array.Copy(currentGeneration, stateGeneration, stateGeneration.Length);

            }
            else if ((string)startButton.Content == "Пауза")
            {
                startButton.Content = "Продолжить";
                newGameButton.IsEnabled = true;
                LoadGameButton.IsEnabled = true;
                saveGame.IsEnabled = true;
                deleteButton.IsEnabled = false;
                timer.Stop();
            }
            else if ((string)startButton.Content == "Продолжить")
            {
                LoadGameButton.IsEnabled = false;
                newGameButton.IsEnabled = false;
                saveGame.IsEnabled = false;
                deleteButton.IsEnabled = false;
                startButton.Content = "Пауза";
                timer.Start();
            }
            

        }

        //Инициализация таймера
        void InitTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            //timer.Start();
        }


        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (TorUniv.IsChecked.Value)
            {
                NextGen();
            }
            else
            {
                NextGen2();
            }
        }



        //Выбор случайного состояния из сохранений БД.
        private void RandomState_Click(object sender, RoutedEventArgs e)
        {
            
            int count = statesTable.Rows.Count;
            Random rng = new Random();
            int row;
            row = rng.Next(count);
            LoadState(row);
            Redraw();
        }

        //Генерация случайного начального состояния поля.
        private void Randomize(object sender, RoutedEventArgs e)
        {
            //Label1.Content = "";

            Random rng = new Random();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    currentGeneration[i, j] = rng.Next(2);
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

        //Пауза игры
        void Stop()
        {
            timer.Stop();
           
        }

        //Перерисовка игрового поля
        void Redraw()
        {
            //Label1.Content = "";
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    string el = "c" + i.ToString() + "_" + j.ToString();
                    //Label1.Content += currentGeneration[i, j].ToString() + "   ";
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
                //Label1.Content += "\r\n";
                Gen_label.Content = "Поколение : " + genCount.ToString();
            }
        }


        //Обработчик события, отвечающий за ручную расстановку клеток начального состояния
        void Change(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            //MessageBox.Show(btn.Name);
            string name = btn.Name;
            //string tmp = name.Substring(name.IndexOf('l') + 2, name.IndexOf('_') - name.IndexOf('l') - 2);
            int i = Int32.Parse(name.Substring(name.IndexOf('c') + 1, name.IndexOf('_') - 1));
            int j = Int32.Parse(name.Substring(name.IndexOf('_') + 1));
            if (btn.Background == Brushes.Black)
            {
                btn.Background = Brushes.White;
                currentGeneration[i, j] = 0;
            }
            else
            {
                btn.Background = Brushes.Black;
                currentGeneration[i, j] = 1;
            }
        }

        //Удаление сохраненной игры
        private void DeleteSave()
        {
          
            int row = dataGrid.SelectedIndex;
        
            savesTable.DefaultView.Delete(row);

            SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
            adapter.Update(savesTable);
        }


        //Сохранение текущей игры в БД
        private void Save(object sender, RoutedEventArgs e)
        {



            SqlConnection connection = null;
            SqlCommand command = new SqlCommand();
            try
            {
                if (SaveTitle.Text.Trim() == "") throw new Exception("Заполните имя сохранения!");
                string title = SaveTitle.Text;

                string state = ConvertBoard(currentGeneration);
                string sstate = ConvertBoard(stateGeneration);
                string lastmodified = DateTime.Now.ToString();
                
                DataRowView rowView = savesTable.DefaultView.AddNew();
                rowView["title"] = title.Trim();
                rowView["state"] = state;
                rowView["lastmodified"] = lastmodified;
                rowView["gen"] = genCount.ToString();
                rowView["datestart"] = dateStart;
                rowView["startstate"] = sstate;
                rowView.EndEdit();
                
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
                adapter.Update(savesTable);
                savesTable.Clear();
                adapter.Fill(savesTable);

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

        //Обработчик события при инициализации окна MainWindow
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            InitTimer();
            string sql = "SELECT * FROM saves where gen > 0";
            string sql2 = "SELECT * FROM saves where gen = 0";
            string sql3 = "SELECT * FROM logs";
            savesTable = new DataTable();
            statesTable = new DataTable();
            logTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand(sql, connection);
                SqlCommand command2 = new SqlCommand(sql2, connection);
                SqlCommand command3 = new SqlCommand(sql3, connection);
                adapter = new SqlDataAdapter(command);
                adapter2 = new SqlDataAdapter(command2);
                adapter3 = new SqlDataAdapter(command3);


                connection.Open();
                adapter.Fill(savesTable);
                adapter2.Fill(statesTable);
                adapter3.Fill(logTable);
                stateGrid.ItemsSource = statesTable.DefaultView;
                dataGrid.ItemsSource = savesTable.DefaultView;
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

            //Label1.Content = "";
            BoardGrid.Children.Clear();
            Random rng = new Random();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    //Label1.Content += currentGeneration[i, j].ToString() + "   ";
                    Button btn1 = new Button();
                    btn1.Name = "c" + i.ToString() + "_" + j.ToString();
                    btn1.Focusable = false;
                    btn1.Click += Change;
                    btn1.Background = Brushes.White;
                    BoardGrid.Children.Add(btn1);
                }
                //Label1.Content += "\r\n";
            }
        }

        //Расчет нового поколения при замкнутом в тор поле.
        void NextGen()
        {
            int neighbours = 0;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (currentGeneration[((sizeX + i - 1) % sizeX), ((sizeY + j - 1) % sizeY)] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i - 1) % sizeX, (sizeY + j) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i - 1) % sizeX, (sizeY + j + 1) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i) % sizeX, (sizeY + j - 1) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i) % sizeX, (sizeY + j + 1) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i + 1) % sizeX, (sizeY + j - 1) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i + 1) % sizeX, (sizeY + j) % sizeY] == 1) neighbours++;
                    if (currentGeneration[(sizeX + i + 1) % sizeX, (sizeY + j + 1) % sizeY] == 1) neighbours++;


                    if (neighbours == 3)
                    {
                        nextGeneration[i, j] = 1;
                    }
                    else
                    {
                        if (neighbours == 2)
                        {
                            if (currentGeneration[i, j] == 1)
                            {
                                nextGeneration[i, j] = 1;
                            }
                            else
                            {
                                nextGeneration[i, j] = 0;
                            }
                        }
                        else
                        {
                            nextGeneration[i, j] = 0;
                        }
                    }
                    neighbours = 0;
                }
            }
            if (Equals(currentGeneration, nextGeneration))
            {
                Stop();
                startButton.IsEnabled = false;
                newGameButton.IsEnabled = true;
                Log();
            }
            Array.Copy(nextGeneration, currentGeneration, nextGeneration.Length);

            genCount++;

            Redraw();
        }

        //Расчет нового поколения при ограниченном поле(за пределом поля жизни нет)
        void NextGen2()
        {
            int neighbours = 0;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (i != 0)
                    {
                        if (j != 0)
                        {
                            if (currentGeneration[i - 1, j - 1] == 1) neighbours++;
                        }
                        if (currentGeneration[i - 1, j] == 1) neighbours++;
                        if (j < sizeY-1)
                        {
                            if (currentGeneration[i - 1, j + 1] == 1) neighbours++;
                        }
                    }
                    if (j != 0)
                    {
                        if (currentGeneration[i, j - 1] == 1) neighbours++;
                        if (j != 0)
                        {
                            if (i < sizeX-1)
                            {
                                if (currentGeneration[i + 1, j - 1] == 1) neighbours++;
                            }
                        }
                    }
                    if (j < sizeY-1)
                    {
                        if (currentGeneration[i, j + 1] == 1) neighbours++;
                    }
                    if (i < sizeX-1)
                    {
                        if (currentGeneration[i + 1, j] == 1) neighbours++;
                    }
                    if (j < sizeY-1)
                    {
                        if (i < sizeX-1)
                        {
                            if (currentGeneration[i + 1, j + 1] == 1) neighbours++;
                        }
                    }


                    if (neighbours == 3)
                    {
                        nextGeneration[i, j] = 1;
                    }
                    else
                    {
                        if (neighbours == 2)
                        {
                            if (currentGeneration[i, j] == 1)
                            {
                                nextGeneration[i, j] = 1;
                            }
                            else
                            {
                                nextGeneration[i, j] = 0;
                            }
                        }
                        else
                        {
                            nextGeneration[i, j] = 0;
                        }
                    }
                    neighbours = 0;
                }
            }
            if (Equals(currentGeneration, nextGeneration))
            {
                Stop();
                startButton.IsEnabled = false;
                newGameButton.IsEnabled = true;
                Log();
            }
            Array.Copy(nextGeneration, currentGeneration, nextGeneration.Length);

            genCount++;

            Redraw();
        }


        bool Equals(int[,] a, int[,] b)
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (a[i, j] != b[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteSave();
            deleteButton.IsEnabled = false;
        }

        //Преобразование состояния массива в строку для записи в БД
        string ConvertBoard(int[,] array)
        {
            string board = "";
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    board += array[i, j].ToString();
                }
            }
            return board;
        }

        //Загрузка состояния игрового поля и номера поколения из сохраненной ранее игры


        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            startButton.Content = "Продолжить";
            startButton.IsEnabled = true;
            randomizeButton.IsEnabled = false;
            randomState.IsEnabled = false;
            LoadStateButton.IsEnabled = false;
            saveStateButton.IsEnabled = false;
            saveGame.IsEnabled = false;
            deleteButton.IsEnabled = false;
            deleteStateButton.IsEnabled = false;
            foreach (UIElement el in BoardGrid.Children)
            {
                Button btn1 = (Button)el;
                btn1.Click -= Change;
            }
            LoadBoard();
            Redraw();
        }
        
        
        void LoadBoard()
        {

            int row = dataGrid.SelectedIndex;
            string board = (string)savesTable.DefaultView[row].Row.ItemArray[2];
            string sboard = (string)savesTable.DefaultView[row].Row.ItemArray[6];
            {
                //string board = "";
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        currentGeneration[i, j] = (int)Char.GetNumericValue(board, 0);
                        stateGeneration[i, j] = (int)Char.GetNumericValue(sboard, 0);
                        board = board.Substring(1);
                        sboard = sboard.Substring(1);
                    }
                }

            }
            int gen = (int)savesTable.DefaultView[row].Row.ItemArray[4];
            dateStart = (DateTime)savesTable.DefaultView[row].Row.ItemArray[5];
            genCount = gen;


        }




        private void dataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if (startButton.Content != "Пауза")
            {
                deleteButton.IsEnabled = true;
            }
        }

        private void stateGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if (startButton.Content == "Старт")
            {
                deleteStateButton.IsEnabled = true;
            }
        }


        //Сохранение нового начального состояния (расстановки) в БД + проверка на наличие такого же состояния в прошлых сохранениях.
        private void SaveState(object sender, RoutedEventArgs e)
        {


            SqlConnection connection = null;
            SqlCommand command = new SqlCommand();
            try
            {
                if (StateTitle.Text.Trim() == "") throw new Exception("Заполните имя сохранения!");

                string title = StateTitle.Text;

                string state = ConvertBoard(currentGeneration);

                int count = statesTable.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    if (state == (string)statesTable.DefaultView[i].Row.ItemArray[2])
                    {
                        throw new Exception("Такое начальное состояние уже есть в базе. \r\nИмя: " + (string)statesTable.DefaultView[i].Row.ItemArray[1] + " Дата создания: " + statesTable.DefaultView[i].Row.ItemArray[3].ToString());
                    }
                }


                string lastmodified = DateTime.Now.ToString();
                
                DataRowView rowView = statesTable.DefaultView.AddNew();
                rowView["title"] = title.Trim();
                rowView["state"] = state;
                rowView["lastmodified"] = lastmodified;
                rowView["gen"] = 0;
                rowView.EndEdit();
                
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter2);
                adapter2.Update(statesTable);
                statesTable.Clear();
                adapter2.Fill(statesTable);

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

        //Загрузка сохраненной начальной расстановки.

        private void LoadStateButton_Click(object sender, RoutedEventArgs e)
        {
            LoadState(stateGrid.SelectedIndex);
            Redraw();
        }


        void LoadState(int row)
        {
            string board = (string)statesTable.DefaultView[row].Row.ItemArray[2];
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
            genCount = 0;
        }

        //Удаление одной из сохраненных начальных расстановок
        private void deleteStateButton_Click(object sender, RoutedEventArgs e)
        {
            int row = stateGrid.SelectedIndex;
            statesTable.DefaultView.Delete(row);

            SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter2);
            adapter2.Update(statesTable);
            deleteStateButton.IsEnabled = false;
        }

        //Начало новой игры.
        void NewGame()
        {
            randomizeButton.IsEnabled = true;
            randomState.IsEnabled = true;
            LoadStateButton.IsEnabled = true;
            startButton.IsEnabled = true;
            LoadGameButton.IsEnabled = true;
            saveStateButton.IsEnabled = true;
            newGameButton.IsEnabled = false;
            genCount = 0;

            startButton.Content = "Старт";
            foreach (UIElement el in BoardGrid.Children)
            {
                Button btn1 = (Button)el;
                btn1.Click += Change;
            }
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    currentGeneration[i, j] = 0;
                }
            }
            Redraw();

        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();


        }

        //Открытия окна лога
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 taskWindow = new Window1();
            taskWindow.Owner = this;
            taskWindow.Show();
        }

        void Log()
        {

            SqlConnection connection = null;
            SqlCommand command = new SqlCommand();
            try
            {
                string sstate = ConvertBoard(stateGeneration);
                string estate = ConvertBoard(nextGeneration);


                string dateend = DateTime.Now.ToString();
                
                DataRowView rowView = logTable.DefaultView.AddNew();
                rowView["startstate"] = sstate;
                rowView["startdate"] = dateStart;
                rowView["endstate"] = estate;
                rowView["enddate"] = dateend;
                rowView.EndEdit();
                
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter3);
                adapter3.Update(logTable);
                logTable.Clear();
                adapter3.Fill(logTable);

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



    }
}
