using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintProgram
{
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private Point startPoint;
        private SolidColorBrush drawingColor = Brushes.Black;
        private List<Curve> curves = new List<Curve>(); // Przechowywanie krzywych
        private Stack<Curve> undoneCurves = new Stack<Curve>(); // Przechowywanie cofniętych krzywych
        private Curve currentCurve; // Obecnie rysowana krzywa

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDrawing = true;
                startPoint = e.GetPosition(canvas);

                currentCurve = new Curve();
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // Tworzenie linii na podstawie aktualnego ruchu myszy
                Line line = new Line()
                {
                    Stroke = drawingColor,
                    StrokeThickness = 2,
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = e.GetPosition(canvas).X,
                    Y2 = e.GetPosition(canvas).Y
                };

                startPoint = e.GetPosition(canvas);

                // Dodawanie linii do aktualnej krzywej i wyświetlanie jej na canvasie
                currentCurve.Lines.Add(line);
                canvas.Children.Add(line);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;

                if (currentCurve.Lines.Count > 0)
                {
                    curves.Add(currentCurve); // Dodawanie zakończonej krzywej do kolekcji
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // Okno dialogowe zapisu pliku
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "drawing";
            dialog.DefaultExt = ".lwp";
            dialog.Filter = "LWP files (*.lwp)|*.lwp";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string fileName = dialog.FileName;
                SaveDrawingToFile(fileName); // Zapisywanie rysunku do pliku
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            // Okno dialogowe wyboru pliku do otwarcia
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".lwp";
            dialog.Filter = "LWP files (*.lwp)|*.lwp";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string fileName = dialog.FileName;
                LoadDrawingFromFile(fileName); // Wczytywanie rysunku z pliku
            }
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (curves.Count > 0)
            {
                Curve lastCurve = curves[curves.Count - 1];

                // Usuwanie linii z canvasa dla ostatniej krzywej
                canvas.Children.RemoveRange(canvas.Children.Count - lastCurve.Lines.Count, lastCurve.Lines.Count);
                curves.RemoveAt(curves.Count - 1); // Usuwanie ostatniej krzywej z kolekcji
                undoneCurves.Push(lastCurve); // Dodawanie usuniętej krzywej do kolekcji cofniętych krzywych
            }
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if (undoneCurves.Count > 0)
            {
                Curve lastUndoneCurve = undoneCurves.Pop();

                curves.Add(lastUndoneCurve); // Dodawanie cofniętej krzywej z powrotem do kolekcji
                foreach (Line line in lastUndoneCurve.Lines)
                {
                    canvas.Children.Add(line); // Wyświetlanie linii na canvasie dla cofniętej krzywej
                }
            }
        }

        private void SaveDrawingToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                string previousLine = "";
                foreach (Curve curve in curves)
                {
                    foreach (Line line in curve.Lines)
                    {
                        // Zapisywanie współrzędnych linii do pliku
                        string newLine = $"{(int)Math.Round(line.X1)};" +
                            $"{(int)Math.Round(line.Y1)};" +
                            $"{(int)Math.Round(line.X2)};" +
                            $"{(int)Math.Round(line.Y2)}";
                        if (newLine != previousLine)
                        {
                            writer.WriteLine(newLine);
                            previousLine = newLine;
                        }
                    }
                    writer.WriteLine("@next"); // Oznaczenie końca krzywej w pliku
                }
            }

            MessageBox.Show("Drawing saved successfully!");
        }

        private void LoadDrawingFromFile(string fileName)
        {
            ClearCanvas();
            curves.Clear();
            undoneCurves.Clear();

            Curve currentCurve = null;

            using (StreamReader reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "@next")
                    {
                        if (currentCurve != null && currentCurve.Lines.Count > 0)
                        {
                            curves.Add(currentCurve); // Dodawanie wczytanej krzywej do kolekcji
                            currentCurve = null;
                        }
                    }
                    else
                    {
                        string[] coordinates = line.Split(';');
                        if (coordinates.Length == 4)
                        {
                            double x1 = double.Parse(coordinates[0]);
                            double y1 = double.Parse(coordinates[1]);
                            double x2 = double.Parse(coordinates[2]);
                            double y2 = double.Parse(coordinates[3]);

                            Line loadedLine = new Line()
                            {
                                Stroke = drawingColor,
                                StrokeThickness = 2,
                                X1 = x1,
                                Y1 = y1,
                                X2 = x2,
                                Y2 = y2
                            };

                            canvas.Children.Add(loadedLine); // Wyświetlanie wczytanej linii na canvasie

                            if (currentCurve == null)
                            {
                                currentCurve = new Curve();
                            }
                            currentCurve.Lines.Add(loadedLine); // Dodawanie wczytanej linii do aktualnej krzywej
                        }
                    }
                }

                if (currentCurve != null && currentCurve.Lines.Count > 0)
                {
                    curves.Add(currentCurve); // Dodawanie ostatniej wczytanej krzywej do kolekcji
                }
            }

            MessageBox.Show("Drawing loaded successfully!");
        }

        private void ClearCanvas()
        {
            canvas.Children.Clear();
        }
    }

    public class Curve
    {
        public List<Line> Lines { get; }

        public Curve()
        {
            Lines = new List<Line>();
        }
    }
}
