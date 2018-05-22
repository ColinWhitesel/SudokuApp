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

namespace SudokuApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //create grid
        Grid sudokuLayout = new Grid
        {
            Height = 300,
            Width = 600,

            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            ShowGridLines = true,
            Margin = new Thickness(10),
        };

        private TextBox[][] userInput = CreateJaggedArray<TextBox[][]>(9, 9);
        private Boolean puzzleSolved;

        //define columns and rows
        private ColumnDefinition[] cols = new ColumnDefinition[9];
        private RowDefinition[] rows = new RowDefinition[9];


        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Sudoku Viewer";

            Border b = new Border()
            {
                BorderThickness = new Thickness()
                {
                    Bottom = 1,
                    Left = 1,
                    Right = 1,
                    Top = 1
                },
                BorderBrush = new SolidColorBrush(Colors.Black)
            };

            b.SetValue(Grid.ColumnSpanProperty, 9);
            b.SetValue(Grid.RowSpanProperty, 9);

            sudokuLayout.Children.Add(b);
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = new ColumnDefinition();
                sudokuLayout.ColumnDefinitions.Add(cols[i]);
                rows[i] = new RowDefinition();
                sudokuLayout.RowDefinitions.Add(rows[i]);
            }

            for (int i = 0; i < cols.Length; i++)
            {
                for (int j = 0; j < rows.Length; j++)
                {
                    userInput[i][j] = new TextBox()
                    {
                        Name = "input" + i + "x" + j,
                    };
                    TextBox temp = new TextBox();
                    Grid.SetColumn(userInput[i][j], i);
                    Grid.SetRow(userInput[i][j], j);

                    sudokuLayout.Children.Add(userInput[i][j]);
                }
            }

            Button solveB = new Button()
            {
                Content = "Solve",
                Width = 100,
            };
            solveB.Click += SolveClicked;
            Button clearB = new Button()
            {
                Content = "Clear Puzzle",
                Width = 100
            };
            clearB.Click += Clear;
            StackPanel panel = new StackPanel();
            panel.Children.Add(new Label { Content = "Sudoku Solver" });
            panel.Children.Add(sudokuLayout);
            panel.Children.Add(clearB);
            panel.Children.Add(solveB);
            this.Content = panel;
            this.Show();
        }

        void SolveClicked(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("Method: solveButtonClicked() Function: ");
            // new array for manipulating textfield inputs
            int[][] userPuzzle = CreateJaggedArray<int[][]>(9, 9);
            userPuzzle = RetrieveInput();

            // if the user's puzzle is invalid, reject it and ask again
            if (!VerifyInput(userPuzzle))
            {
                MessageBox.Show("Invalid Sudoku Puzzle Entered.");
                ClearInput();
            }
 
            puzzleSolved = Solve(0, 0, userPuzzle);

            // if solve() returns false:
            // solver failed. return 0 to switch statement for case0
            // if solve() returns true:
            // puzzle solve. return 1 to switch statement for case 1
            // display solved puzzle to user
            if (puzzleSolved == false)
            {
                MessageBox.Show("Invalid Sudoku Puzzle Entered.");
                ClearInput();
            }
            else
            {
                UpdatePuzzle(userPuzzle);
                MessageBox.Show("Solution to inputed Sudoku Puzzle");
            }
        }

        void Clear(object sender, RoutedEventArgs e)
        {
            ClearInput();
        }

        // method that starts solving process
        Boolean Solve(int i, int j, int[][] cells)
        {
            //Console.WriteLine("Method: solve() Function: ");

            if (i == 9)
            {
                i = 0;
                if (++j == 9)
                    return true;
            }
            // skip filled cells
            if (cells[i][j] != 0)
                return Solve(i + 1, j, cells);

            for (int val = 1; val <= 9; ++val)
            {
                if (Legal(i, j, val, cells))
                {
                    cells[i][j] = val;
                    if (Solve(i + 1, j, cells))
                        return true;
                }
            }

            // reset on backtrack
            cells[i][j] = 0;
            return false;
        }

        //currently not implemented, function attempts to create
        //a valid sudoku puzzle through brute force algorithm due to time
        //and complexity of a sudoku puzzle
        public int[][] GeneratePuzzle()
        {
            int[][] tempPuzzle;
            Random chaos = new Random();

            while (true)
            {
                tempPuzzle = CreateJaggedArray<int[][]>(9, 9);
                //determines how many values are generated
                int randCounter = 6;

                while (randCounter > 0)
                {
                    int x = chaos.Next(0, 8+1);
                    int y = chaos.Next(0, 8+1);
                    int value = chaos.Next(1, 9+1);

                    tempPuzzle[x][y] = value;

                    randCounter--;
                }

                int[][] toSolve = CreateJaggedArray<int[][]>(9, 9);
                for (int j = 0; j < userInput.Length; j++)
                {
                    for (int k = 0; k < userInput[0].Length; k++)
                    {
                        toSolve[j][k] = tempPuzzle[j][k];
                    }
                }

                if (!VerifyInput(tempPuzzle))
                {
                    Console.WriteLine("Generation failed. Retrying.");
                    GeneratePuzzle();
                }

                if (Solve(0, 0, toSolve) == true)
                {
                    //Console.WriteLine("done");
                    //printPuzzle(testPuzzle);
                    return tempPuzzle;
                }
            }
        }

        // method determines if a value is legal
        Boolean Legal(int i, int j, int val, int[][] cells)
        {
            // row values
            for (int k = 0; k < 9; ++k)
                if (val == cells[k][j])
                    return false;

            // column values
            for (int k = 0; k < 9; ++k)
                if (val == cells[i][k])
                    return false;

            // 3x3 values
            int boxRowOffset = (i / 3) * 3;
            int boxColOffset = (j / 3) * 3;
            for (int k = 0; k < 3; ++k)
                for (int m = 0; m < 3; ++m)
                    if (val == cells[boxRowOffset + k][boxColOffset + m])
                        return false;

            // no violations found, legal move
            return true;
        }


        // method that verifies if what the user input is a valid puzzle (idiot
        // proofing)
        Boolean VerifyInput(int[][] userPuzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                // three aspects that must be true for valid SUDOKU puzzle
                int[] row = new int[9];
                int[] square = new int[9];
                int[] column = (int[]) userPuzzle[i].Clone();

                for (int j = 0; j < 9; j++)
                {
                    row[j] = userPuzzle[j][i];
                    square[j] = userPuzzle[(i / 3) * 3 + j / 3][i * 3 % 9 + j % 3];
                }

                // if one fails test, puzzle is invalid. Return false so user can try again
                if (!(VerifyComponent(column) && VerifyComponent(row) && VerifyComponent(square)))
                    return false;
            }

            // if everything checks out, code continues on its merry way
            return true;
        }

        // helper method of VerifyInput. When a component, such as the 3x3 box or a row
        // is passed into the method, the array is sorted and if two consecutive items
        // match
        // user's puzzle is invalid
        private Boolean VerifyComponent(int[] component)
        {
            int previous = 0;
            Array.Sort(component);
            foreach (int number in component)
                if (number != 0)
                {
                    if (previous == number)
                        return false;
                    previous = number;
                }
            return true;
        }

        // method that populates userPuzzle[][] with input[][] textfields
        private int[][] RetrieveInput()
        {
            int[][] temp = CreateJaggedArray<int[][]>(9, 9);
            for (int i = 0; i < userInput.Length; i++)
                for (int u = 0; u < userInput[0].Length; u++)
                {
                    if (userInput[i][u].Text.Length == 0)
                        temp[i][u] = 0;
                    else
                    {
                        //takes first character in string
                        char c = userInput[i][u].Text[0];
                        if (c > '0' && c <= '9')
                        {
                            temp[i][u] = c - '0';
                        }
                        //if not 0-9, change to '0'
                        else
                            temp[i][u] = 0;
                    }
                }
            return temp;
        }

        void ClearInput()
        {
            for (int i = 0; i < cols.Length; i++)
            {
                for (int j = 0; j < rows.Length; j++)
                {
                    userInput[i][j].Text = "";
                }
            }
        }

        //debug tool
        void PrintPuzzle<T>(T[][] puzzle)
        {
            Console.WriteLine("==START==");
            for (int i = 0; i < puzzle.Length; i++)
            {
                for (int j = 0; j < puzzle.Length; j++)
                {
                    Console.Write(puzzle[i][j] + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("===END===");
        }
        void UpdatePuzzle(int[][] results)
        {
            for (int i = 0; i < userInput.Length; i++)
            {
                for (int j = 0; j < userInput[0].Length; j++)
                {
                    userInput[i][j].Text = results[i][j].ToString();
                }
            }
        }

        static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }
    }
}