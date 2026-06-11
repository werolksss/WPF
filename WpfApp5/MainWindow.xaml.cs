using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KeyboardTrainer
{
    public partial class MainWindow : Window
    {
        private readonly Random random = new Random();
        private readonly Dictionary<Key, Button> buttonsByKey = new Dictionary<Key, Button>();
        private readonly Dictionary<Button, Brush> originalBrushes = new Dictionary<Button, Brush>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly DispatcherTimer timer = new DispatcherTimer();

        private string task = "";
        private int position = 0;
        private int fails = 0;
        private int correct = 0;
        private bool isStarted = false;
        private bool isShift = false;
        private bool isCaps = false;

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(400);
            timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterKeys();
            UpdateLetterCase();
            Focus();
        }

        private void RegisterKeys()
        {
            foreach (Button button in FindVisualChildren<Button>(this))
            {
                originalBrushes[button] = button.Background;

                if (button.Tag == null)
                    continue;

                string tag = button.Tag.ToString();

                if (Enum.TryParse(tag, out Key key))
                    buttonsByKey[key] = button;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            int difficulty = (int)DifficultySlider.Value;

            task = GenerateText(difficulty);
            position = 0;
            fails = 0;
            correct = 0;
            isStarted = true;

            TaskText.Text = task;
            FailsText.Text = "0";
            SpeedText.Text = "0 chars/min";

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            DifficultySlider.IsEnabled = false;
            CaseSensitiveCheckBox.IsEnabled = false;

            stopwatch.Restart();
            timer.Start();
            Focus();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopTraining();
        }

        private void StopTraining()
        {
            isStarted = false;
            timer.Stop();
            stopwatch.Stop();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            DifficultySlider.IsEnabled = true;
            CaseSensitiveCheckBox.IsEnabled = true;

            ResetAllKeyColors();
        }

        private string GenerateText(int difficulty)
        {
            string alphabet = "fhaip";
            string result = "";

            int length = 25 + difficulty * 12;

            for (int i = 0; i < length; i++)
            {
                if (i > 0 && i % 7 == 0)
                {
                    result += " ";
                    continue;
                }

                char ch = alphabet[random.Next(alphabet.Length)];

                if (CaseSensitiveCheckBox.IsChecked == true && random.Next(5) == 0)
                    ch = char.ToUpper(ch);

                result += ch;
            }

            return result;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.LeftShift || key == Key.RightShift)
            {
                isShift = true;
                UpdateLetterCase();
            }

            if (key == Key.CapsLock)
            {
                isCaps = !isCaps;
                UpdateLetterCase();
            }

            SetKeyColor(key, Brushes.White);

            if (!isStarted)
                return;

            char? typed = GetChar(key);

            if (typed == null)
                return;

            e.Handled = true;

            if (position >= task.Length)
                return;

            char expected = task[position];

            if (typed.Value == expected)
            {
                correct++;
            }
            else
            {
                fails++;
                FailsText.Text = fails.ToString();
            }

            position++;

            string done = task.Substring(0, position);
            string left = task.Substring(position);

            TaskText.Text = done + left;

            if (position >= task.Length)
            {
                StopTraining();
                MessageBox.Show("Упражнение завершено!", "Keyboard Trainer");
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.LeftShift || key == Key.RightShift)
            {
                isShift = false;
                UpdateLetterCase();
            }

            ResetKeyColor(key);
        }

        private char? GetChar(Key key)
        {
            bool upper = isShift ^ isCaps;

            if (key >= Key.A && key <= Key.Z)
            {
                char ch = (char)('a' + (key - Key.A));

                if (CaseSensitiveCheckBox.IsChecked == true && upper)
                    ch = char.ToUpper(ch);

                return ch;
            }

            if (key >= Key.D0 && key <= Key.D9)
                return (char)('0' + (key - Key.D0));

            if (key == Key.Space) return ' ';
            if (key == Key.OemMinus) return '-';
            if (key == Key.OemPlus) return '=';
            if (key == Key.OemOpenBrackets) return '[';
            if (key == Key.Oem6) return ']';
            if (key == Key.Oem5) return '\\';
            if (key == Key.Oem1) return ';';
            if (key == Key.OemQuotes) return '\'';
            if (key == Key.OemComma) return ',';
            if (key == Key.OemPeriod) return '.';
            if (key == Key.OemQuestion) return '/';
            if (key == Key.Oem3) return '`';

            return null;
        }

        private void SetKeyColor(Key key, Brush brush)
        {
            if (buttonsByKey.ContainsKey(key))
                buttonsByKey[key].Background = brush;
        }

        private void ResetKeyColor(Key key)
        {
            if (buttonsByKey.ContainsKey(key))
            {
                Button button = buttonsByKey[key];

                if (originalBrushes.ContainsKey(button))
                    button.Background = originalBrushes[button];
            }
        }

        private void ResetAllKeyColors()
        {
            foreach (var pair in originalBrushes)
                pair.Key.Background = pair.Value;
        }

        private void UpdateLetterCase()
        {
            bool upper = isShift ^ isCaps;

            foreach (var pair in buttonsByKey)
            {
                Key key = pair.Key;

                if (key >= Key.A && key <= Key.Z)
                {
                    string text = pair.Value.Content.ToString();
                    pair.Value.Content = upper ? text.ToUpper() : text.ToLower();
                }
            }
        }

        private void DifficultySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DifficultyText != null)
                DifficultyText.Text = ((int)e.NewValue).ToString();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isStarted || stopwatch.Elapsed.TotalMinutes <= 0)
                return;

            int speed = (int)(correct / stopwatch.Elapsed.TotalMinutes);
            SpeedText.Text = speed + " chars/min";
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    yield return result;

                foreach (T item in FindVisualChildren<T>(child))
                    yield return item;
            }
        }
    }
}
