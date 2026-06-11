using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp4
{
    public partial class MainWindow : Window
    {
        double firstNumber = 0;
        string operation = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string value = ((Button)sender).Content.ToString();

            if (txtDisplay.Text == "0" && value != ".")
            {
                txtDisplay.Text = value;
            }
            else if (value == "." && txtDisplay.Text.Contains("."))
            {
                return;
            }
            else
            {
                txtDisplay.Text += value;
            }
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            firstNumber = Convert.ToDouble(txtDisplay.Text);
            operation = ((Button)sender).Content.ToString();

            txtHistory.Text = firstNumber + " " + operation;
            txtDisplay.Text = "0";
        }

        private void Result_Click(object sender, RoutedEventArgs e)
        {
            double secondNumber = Convert.ToDouble(txtDisplay.Text);
            double result = 0;

            switch (operation)
            {
                case "+":
                    result = firstNumber + secondNumber;
                    break;

                case "-":
                    result = firstNumber - secondNumber;
                    break;

                case "*":
                    result = firstNumber * secondNumber;
                    break;

                case "/":
                    result = firstNumber / secondNumber;
                    break;
            }

            txtHistory.Text += " " + secondNumber + " =";
            txtDisplay.Text = result.ToString();
        }

        private void CE_Click(object sender, RoutedEventArgs e)
        {
            txtDisplay.Text = "0";
        }

        private void C_Click(object sender, RoutedEventArgs e)
        {
            txtDisplay.Text = "0";
            txtHistory.Text = "";
            firstNumber = 0;
            operation = "";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (txtDisplay.Text.Length > 1)
            {
                txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
            }
            else
            {
                txtDisplay.Text = "0";
            }
        }
    }
}