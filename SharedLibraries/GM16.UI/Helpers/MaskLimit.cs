using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace GM16.UI.Helpers
{
    public class MaskLimit
    {
        public static readonly DependencyProperty RegexProperty =
            DependencyProperty.RegisterAttached("Regex", typeof(string), typeof(MaskLimit), new PropertyMetadata(null, OnRegexChanged));

        public static string GetRegex(DependencyObject obj)
        {
            return (string)obj.GetValue(RegexProperty);
        }

        public static void SetRegex(DependencyObject obj, string value)
        {
            obj.SetValue(RegexProperty, value);
        }

        private static void OnRegexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = obj as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged -= TextBox_TextChanged;
                textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;

                string regexPattern = e.NewValue as string;
                if (!string.IsNullOrEmpty(regexPattern))
                {
                    textBox.TextChanged += TextBox_TextChanged;
                    textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string regexPattern = GetRegex(textBox);
            string newText = textBox.Text;

            if (!Regex.IsMatch(newText, regexPattern))
            {
                textBox.TextChanged -= TextBox_TextChanged;
                //textBox.Undo();
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                TextBox textBox = sender as TextBox;
                string regexPattern = GetRegex(textBox);
                string newText = textBox.Text.Insert(textBox.CaretIndex, " ");

                if (!Regex.IsMatch(newText, regexPattern))
                {
                    e.Handled = true;
                }
            }
        }
    }
}
