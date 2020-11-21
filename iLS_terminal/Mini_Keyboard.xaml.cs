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

namespace FreezePunch
{
    /// <summary>
    /// Mini_Keyboard.xaml 的交互逻辑
    /// </summary>
    public partial class Mini_Keyboard : UserControl
    {
        private IInputElement focusedControl;
        public Mini_Keyboard()
        {
            InitializeComponent();
        }

        private void IN_7_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_2_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_3_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_0_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_Dot_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_4_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_5_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_6_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_8_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_9_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void CLEAR_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void SET_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                focusedControl = Keyboard.FocusedElement;
        }

        private void IN_0_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "0";
            }
        }

        private void IN_Dot_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += ".";
            }
        }

        private void IN_1_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "1";
            }
        }

        private void IN_2_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "2";
            }
        }

        private void IN_3_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "3";
            }
        }

        private void IN_4_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "4";
            }
        }

        private void IN_5_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "5";
            }
        }

        private void IN_6_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "6";
            }
        }

        private void IN_7_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "7";
            }
        }

        private void IN_8_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "8";
            }
        }

        private void IN_9_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text += "9";
            }
        }

        private void CLEAR_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
                tb.Text = "";
            }
        }

        private void SET_Click(object sender, RoutedEventArgs e)
        {
            if (focusedControl != null && focusedControl is TextBox)
            {
                TextBox tb = (TextBox)focusedControl;
            }
        }
    }
}
