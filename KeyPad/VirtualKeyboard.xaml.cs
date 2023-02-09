/*
 * Copyright (c) 2008, Andrzej Rusztowicz (ekus.net)
* All rights reserved.

* Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of ekus.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
/*
 * Added by Michele Cattafesta (mesta-automation.com) 29/2/2011
 * The code has been totally rewritten to create a control that can be modified more easy even without knowing the MVVM pattern.
 * If you need to check the original source code you can download it here: http://wosk.codeplex.com/
 */

/*
 * Added by Charles Chang on Feb.9, 2023.
 * 
 * Based the code by Michele Cattafesta, changed button style and fixed window size as 800 x 600.
 * 
 * On screen keypad and keyboard for touch screen with resolution 800 x 600.
 */

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
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace KeyPad
{
    /// <summary>
    /// Logica di interazione per VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : Window, INotifyPropertyChanged
    {
        #region Public Properties

        private bool _showNumericKeyboard;
        public bool ShowNumericKeyboard
        {
            get { return _showNumericKeyboard; }
            set { _showNumericKeyboard = value; this.OnPropertyChanged("ShowNumericKeyboard"); }
        }

        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; this.OnPropertyChanged("Result"); }
        }

        private Thumb Thumb
        {
            get
            {
                return GetThumb(slCursorHandle) as Thumb; ;
            }
        }

        #endregion

        #region Constructor

        public bool isCancelPressed = false;
        public bool isEnterPressed = false;
        public bool oneRunOnly = false;
        string lastValue;
        List<string> ListChecker;
        private bool isThumbPressed = false;
        private int caretOffset = 0;
        private double lastCV = 0.00;

        public VirtualKeyboard(TextBox owner, Window wndOwner)
        {
            InitializeComponent();
            this.Owner = wndOwner;
            SetPrimaryScreen();
            this.DataContext = this;
            Result = owner.Text.ToString();
            txtResult.Focus();
            lastValue = Result;
            txtResult.Text = Result;
            ListChecker = new List<string>();
            ListChecker.Add("haha");
            ListChecker.Add("hihi");
            ListChecker.Add("asdasd");
            txtResult.SelectionStart = txtResult.Text.Length;
            slCursorHandle.Ticks = GetTickCollection(Result); ;
        }

        private void SetPrimaryScreen()
        {
            if (SystemParameters.PrimaryScreenHeight > 600 &&
               SystemParameters.PrimaryScreenWidth > 800)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                this.Height = 600;
                this.Width = 800;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }
        #endregion

        public void KeyPadAnimation(Grid grd, int to, int from, int timing)
        {
            Storyboard sb = new Storyboard();

            DoubleAnimation slide = new DoubleAnimation();
            slide.To = to;
            slide.From = from;
            slide.Duration = new Duration(TimeSpan.FromMilliseconds(timing));

            Storyboard.SetTarget(slide, grd);
            Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));

            sb.Children.Add(slide);
            sb.Completed += new EventHandler(Story_Completed);
            sb.Begin();

            if (isCancelPressed || isEnterPressed)
            {
                DoubleAnimation da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
                grd.BeginAnimation(Grid.OpacityProperty, da);
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
                grd.BeginAnimation(Grid.OpacityProperty, da);
            }
        }

        void Story_Completed(object sender, EventArgs e)
        {
            if (isCancelPressed && oneRunOnly)
            {
                this.DialogResult = false;
                oneRunOnly = false;
            }
            else if (isEnterPressed && oneRunOnly)
            {
                this.DialogResult = true;
                oneRunOnly = false;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            slCursorHandle.Visibility = Visibility.Hidden;
            Button button = sender as Button;
            if (button != null && txtResult.GetLineLength(0) < 250)
            {
                switch (button.CommandParameter.ToString())
                {
                    case "LSHIFT":
                        Regex upperCaseRegex = new Regex("[A-Z]");
                        Regex lowerCaseRegex = new Regex("[a-z]");
                        Button btn;
                        foreach (UIElement elem in AlfaKeyboard.Children) //iterate the main grid
                        {
                            Grid grid = elem as Grid;
                            if (grid != null)
                            {
                                foreach (UIElement uiElement in grid.Children) //iterate the single rows
                                {
                                    btn = uiElement as Button;
                                    if (btn != null) // if button contains only 1 character
                                    {
                                        if (btn.Content.ToString().Length == 1)
                                        {
                                            if (upperCaseRegex.Match(btn.Content.ToString()).Success) // if the char is a letter and uppercase
                                                btn.Content = btn.Content.ToString().ToLower();
                                            else if (lowerCaseRegex.Match(button.Content.ToString()).Success) // if the char is a letter and lower case
                                                btn.Content = btn.Content.ToString().ToUpper();
                                        }
                                    }
                                }

                                if (btnA.Content.ToString() == "A")
                                {
                                    this.Resources["shiftOn"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-on@2x.png")));
                                    this.Resources["shiftOnPressed"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-on-pressed@2x.png")));
                                    this.Resources["shiftOnDisabled"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-on-disable@2x.png")));
                                }
                                else
                                {
                                    this.Resources["shiftOn"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-off@2x.png")));
                                    this.Resources["shiftOnPressed"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-off-pressed@2x.png")));
                                    this.Resources["shiftOnDisabled"] = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/KeyPad;component/Resources/assets_keyboard/btn-key-shift-off-disable@2x.png")));
                                }
                            }
                        }
                        break;

                    case "RETURN":
                        if (ListChecker != null)
                        {
                            if (ListChecker.Contains(Result))
                            {

                            }
                            else
                            {
                                isEnterPressed = true;
                                KeyPadAnimation(grdMid, 40, 0, 250);
                                KeyPadAnimation(grdBot, 100, 0, 300);
                                this.DialogResult = true;
                            }
                        }
                        else
                        {
                            isEnterPressed = true;
                            KeyPadAnimation(grdMid, 40, 0, 250);
                            KeyPadAnimation(grdBot, 100, 0, 300);
                            this.DialogResult = true;
                        }
                        break;

                    case "BACK":
                        txtResult.Focus(); // set the textbox focused.

                        string curText = Result;
                        string delText = "";

                        if (txtResult.SelectionStart >= 0) //check if there is text to delete
                        {
                            int TxTindex = txtResult.SelectionStart; // save the index position

                            if (txtResult.SelectedText.Length > 0)  //// check if there is selected text
                            {
                                delText = txtResult.SelectedText;
                                txtResult.SelectedText = "";
                                Result = txtResult.Text;
                            }
                            else if (TxTindex > 0) // check if there is text in texbox
                            {
                                delText = Result.Substring(TxTindex - 1, 1);
                                Result = Result.Remove(TxTindex - 1, 1);
                                txtResult.SelectionStart = TxTindex - 1; // to set the cursor position after the deleted number between the text.
                            }
                        }

                        slCursorHandle.Ticks.Clear();
                        slCursorHandle.Ticks = GetTickCollection(Result);
                        break;

                    case "CLR":
                        Result = "";
                        break;

                    case "SYMCHANGE":
                        grdSymbolPage2.Visibility = grdSymbolPage2.Visibility == System.Windows.Visibility.Hidden ?
                            grdSymbolPage2.Visibility = System.Windows.Visibility.Visible :
                            grdSymbolPage2.Visibility = System.Windows.Visibility.Hidden;
                        break;

                    case "ESC":
                        isCancelPressed = true;
                        KeyPadAnimation(grdMid, 40, 0, 250);
                        KeyPadAnimation(grdBot, 100, 0, 300);
                        oneRunOnly = true;
                        break;

                    default:
                        txtResult.Focus();

                        if (txtResult.SelectedText.Length > 0)
                            txtResult.SelectedText = "";
                        int TxTindex1 = txtResult.SelectionStart;
                        string subString1 = txtResult.Text.Substring(0, TxTindex1);
                        string subString2 = txtResult.Text.Substring(TxTindex1, txtResult.Text.Length - subString1.Length);
                        string combinedString = subString1 + button.Content.ToString() + subString2;

                        //System.Console.WriteLine(combinedString);

                        //Result += button.Content.ToString();

                        Result = combinedString;
                        txtResult.SelectionStart = TxTindex1 + 1;
                        slCursorHandle.Ticks.Clear();
                        slCursorHandle.Ticks = GetTickCollection(Result);
                        break;
                }
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void txtResult_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void txtResult_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int start = txtResult.SelectionStart;
            int len = txtResult.SelectionLength;

            if (txtResult.CaretIndex == 0)
            {
                slCursorHandle.Ticks = GetTickCollection(Result);
                slCursorHandle.Value = slCursorHandle.Minimum;
                slCursorHandle.Visibility = System.Windows.Visibility.Visible;
            }
            else if (Math.Round(txtResult.HorizontalOffset, 2) == slCursorHandle.Minimum
                && slCursorHandle.Ticks[txtResult.CaretIndex - 1].CompareTo(slCursorHandle.Maximum) <= 0)
            {
                slCursorHandle.Ticks = GetTickCollection(Result);
                slCursorHandle.Value = (slCursorHandle.Ticks[txtResult.CaretIndex - 1] < 0 ? 0
                    : slCursorHandle.Ticks[txtResult.CaretIndex - 1]);
                slCursorHandle.Visibility = System.Windows.Visibility.Visible;
                caretOffset = 1;
            }
            else if (Math.Round(txtResult.HorizontalOffset, 2) > 0)
            {
                for (int i = 1; i <= Result.Length; i++)
                {
                    double mes = MeasureString(Result.Substring(0, i));
                    
                    if (Math.Round(txtResult.HorizontalOffset, 2) < mes)
                    {
                        slCursorHandle.Ticks.Clear();
                        slCursorHandle.Ticks = GetTickCollection(Result.Substring(i, Result.Length - i), mes - Math.Round(txtResult.HorizontalOffset, 2));
                        slCursorHandle.Value = slCursorHandle.Ticks[txtResult.CaretIndex - i];
                        slCursorHandle.Visibility = System.Windows.Visibility.Visible;
                        caretOffset = i;
                        break;
                    }
                }
            }
            else
            {
                slCursorHandle.Ticks = GetTickCollection(Result);
                slCursorHandle.Value = slCursorHandle.Maximum;
                slCursorHandle.Visibility = System.Windows.Visibility.Visible;
            }
            
            txtResult.Select(start, len);
        }

        private double MeasureString(string result)
        {
            var formattedText = new FormattedText(
                result,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(txtResult.FontFamily, txtResult.FontStyle, txtResult.FontWeight, txtResult.FontStretch),
                txtResult.FontSize,
                Brushes.White,
                new NumberSubstitution());

            return Math.Round(formattedText.WidthIncludingTrailingWhitespace, 2);
        }

        private DoubleCollection GetTickCollection(string result, double offset = 0.00)
        {
            DoubleCollection col = new DoubleCollection();

            if (offset > 0)
                col.Add(Math.Round(offset, 2));

            for (int i = 1; i <= result.Length; i++)
            {
                var formattedText = new FormattedText(
                result.Substring(0, i),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(txtResult.FontFamily, txtResult.FontStyle, txtResult.FontWeight, txtResult.FontStretch),
                txtResult.FontSize,
                Brushes.White,
                new NumberSubstitution());

                if (Math.Round(formattedText.WidthIncludingTrailingWhitespace + offset, 2) > slCursorHandle.Maximum)
                    break;
                else
                    col.Add(Math.Round(formattedText.WidthIncludingTrailingWhitespace + offset, 2));
            }
            return col;
        }

        private void slCursorHandle_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isThumbPressed = true;
        }

        private void slCursorHandle_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (Math.Round(txtResult.HorizontalOffset, 2) > 0)
            {
                if (lastCV == slCursorHandle.Minimum && slCursorHandle.Value == slCursorHandle.Minimum)
                {
                    while (Thumb.IsDragging && slCursorHandle.Value == slCursorHandle.Minimum 
                        && e.HorizontalChange < -15 && caretOffset >= 1)
                    {
                        double diff = MeasureString(Result.Substring(0, caretOffset - 1));

                        txtResult.ScrollToHorizontalOffset(diff);
                        txtResult.CaretIndex = caretOffset - 1;
                        slCursorHandle.Ticks.Clear();
                        slCursorHandle.Ticks = GetTickCollection(Result.Substring(caretOffset - 1, Result.Length - caretOffset));
                        slCursorHandle.Value = slCursorHandle.Minimum;
                        caretOffset--;
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                else if (lastCV == slCursorHandle.Maximum && slCursorHandle.Value == slCursorHandle.Maximum)
                {
                    while (Thumb.IsDragging && slCursorHandle.Value == slCursorHandle.Maximum 
                        && e.HorizontalChange > 15 && txtResult.CaretIndex <= Result.Length - 1)
                    {
                        double diff = MeasureString(Result.Substring(txtResult.CaretIndex, 1));
                        txtResult.CaretIndex += 1;
                        txtResult.ScrollToHorizontalOffset(txtResult.HorizontalOffset + diff);

                        for (int i = 1; i <= Result.Length; i++)
                        {
                            double mes = MeasureString(Result.Substring(0, i));

                            if (Math.Round(txtResult.HorizontalOffset, 2) < mes)
                            {
                                slCursorHandle.Ticks.Clear();
                                slCursorHandle.Ticks = GetTickCollection(Result.Substring(i, Result.Length - i),
                                    mes - Math.Round(txtResult.HorizontalOffset, 2));
                                slCursorHandle.Value = slCursorHandle.Maximum;
                                caretOffset = i + 1;
                                break;
                            }
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        private void slCursorHandle_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isThumbPressed = false;
            txtResult.Focus();
        }

        private DependencyObject GetThumb(DependencyObject root)
        {
            if (root is Thumb)
                return root;

            DependencyObject thumb = null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                thumb = GetThumb(VisualTreeHelper.GetChild(root, i));

                if (thumb is Thumb)
                    return thumb;
            }

            return thumb;
        }

        private void slCursorHandle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isThumbPressed)
            {
                if (Math.Round(slCursorHandle.Value, 2) >= Math.Round(MeasureString(Result), 2))
                {
                    slCursorHandle.Value = MeasureString(Result);
                    txtResult.CaretIndex = slCursorHandle.Ticks.Count;
                    return;
                }

                if (MeasureString(Result) > slCursorHandle.Maximum)
                {
                    if (slCursorHandle.Value == slCursorHandle.Minimum)
                    {
                        double diff = MeasureString(Result.Substring(0, caretOffset - 1));

                        txtResult.ScrollToHorizontalOffset(diff);
                        txtResult.CaretIndex = caretOffset - 1;
                        slCursorHandle.Ticks.Clear();
                        slCursorHandle.Ticks = GetTickCollection(Result.Substring(caretOffset - 1, Result.Length - caretOffset));
                        slCursorHandle.Value = slCursorHandle.Minimum;
                        caretOffset--;
                    }
                    else if (slCursorHandle.Value == slCursorHandle.Maximum)
                    {
                        double y = 0.00;
                        for (int x = 1; x <= Result.Length; x++)
                        {
                            double yy = MeasureString(Result.Substring(0, x));

                            if (Math.Round(yy - txtResult.HorizontalOffset, 2) < slCursorHandle.Maximum)
                                y = Math.Round(yy - txtResult.HorizontalOffset, 2);
                            else
                                break;
                        }

                        double off = (MeasureString(Result.Substring(txtResult.CaretIndex, 1)) 
                            > Math.Round(slCursorHandle.Maximum - y, 2))
                            ? (MeasureString(Result.Substring(txtResult.CaretIndex, 1))
                            - Math.Round(slCursorHandle.Maximum - y, 2))
                            : MeasureString(Result.Substring(txtResult.CaretIndex, 1));

                        txtResult.CaretIndex += 1;
                        txtResult.ScrollToHorizontalOffset(txtResult.HorizontalOffset + off);

                        for (int i = 1; i <= Result.Length; i++)
                        {
                            double mes = MeasureString(Result.Substring(0, i));

                            if (Math.Round(txtResult.HorizontalOffset, 2) < mes)
                            {
                                slCursorHandle.Ticks.Clear();
                                slCursorHandle.Ticks = GetTickCollection(Result.Substring(i, Result.Length - i), 
                                    mes - Math.Round(txtResult.HorizontalOffset, 2));
                                slCursorHandle.Value = slCursorHandle.Maximum;
                                caretOffset = i + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (lastCV == slCursorHandle.Minimum)
                        {
                            caretOffset++;
                        }
                        else if (lastCV == slCursorHandle.Maximum)
                        {
                            caretOffset--;
                        }

                        txtResult.CaretIndex = slCursorHandle.Ticks.IndexOf(slCursorHandle.Value) + caretOffset;
                    }
                }
                else
                {
                    txtResult.CaretIndex = slCursorHandle.Ticks.IndexOf(slCursorHandle.Value) + 1;
                }
            }

            lastCV = slCursorHandle.Value;
        }

    }
}
