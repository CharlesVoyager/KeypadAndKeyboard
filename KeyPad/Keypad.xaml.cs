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
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace KeyPad
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class Keypad : Window, INotifyPropertyChanged
    {
        #region Public Properties

        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; this.OnPropertyChanged("Result"); }
        }
        #endregion

        private string defaultResult = "";
        public double minKeypadValue = 0;
        public double maxKeypadValue = 100;
        public double dblResult = 0;
        public double dblResultX = 0;
        public double dblResultY = 0;
        public double dblResultZ = 0;
        public bool isCancelPressed = false;
        public bool isEnterPressed = false;
        public bool oneRunOnly = false;
        private bool validationEnabled = false;

        private int countDecimalDigits(string number)
        {
            int posPoint = number.IndexOf(".");

            if (posPoint == -1) // Not found "."
                return 0;   
            else
                return number.Length - (posPoint + 1);
        }

        // EX: 56.234324 -> 2
        //     12345 -> 5
        private int countDigits(string number)
        {
            int posPoint = number.IndexOf(".");

            if (posPoint == -1) // Not found "."
                return number.Length;
            else
                return posPoint;
        }

        private bool isOverDigits(string number)
        {
            return false;
        }

        public Keypad(TextBox owner, Window wndOwner)
        {
            InitializeComponent();

            Owner = wndOwner;
            SetPrimaryScreen();
            brdMain.Visibility = Visibility.Visible;
            DataContext = this; 
            Result = owner.Text.ToString();
            defaultResult = Result;
            txtResult.Focus();
            txtResult.Text = Result;
            txtResult.SelectionStart = txtResult.Text.Length;
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
        public Keypad(TextBox owner)
        {
            InitializeComponent();
            SetPrimaryScreen();
            brdMain.Visibility = Visibility.Visible;
            DataContext = this;
            Result = owner.Text.ToString();
            defaultResult = Result;
            btnDecimal.IsEnabled = true;
            KeyPadAnimation(grdMid, 0, 40, 250);
            KeyPadAnimation(grdBot, 0, 100, 300);
            txtResult.Focus();
            txtResult.Text = Result;
            txtResult.SelectionStart = txtResult.Text.Length;
        }

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

            if (Result != "")
            {
                if (Result == ".")
                {
                    dblResult = 0.0;
                }
                else if (Result.Contains("."))
                {
                    dblResult = Convert.ToDouble(Result);
                }
                else
                {
                    dblResult = Convert.ToDouble(Result);
                }
            }
            Button button = sender as Button;
            switch (button.CommandParameter.ToString()) 
            {
                case "ESC": // Cancel
                    isCancelPressed = true;
                    KeyPadAnimation(grdMid, 40, 0, 250);
                    KeyPadAnimation(grdBot, 100, 0, 300);
                    oneRunOnly = true;
                    break;

                case "RETURN": // Enter
                    if (Result.EndsWith("."))
                    {
                        Result = Convert.ToString(dblResult);
                    }
                    else if (Result.StartsWith("."))
                    {
                        Result = "0" + Convert.ToString(dblResult);
                    }

                    if ((dblResult > maxKeypadValue) && validationEnabled)
                    {
                        Result = maxKeypadValue.ToString();
                    }
                    else if ((dblResult < minKeypadValue) && validationEnabled)
                    {
                        Result = minKeypadValue.ToString();
                    }
                    else if (String.IsNullOrEmpty(Result))
                    {
                    }
                    else
                    {
                        isEnterPressed = true;
                        KeyPadAnimation(grdMid, 40, 0, 250);
                        KeyPadAnimation(grdBot, 100, 0, 300);
                        oneRunOnly = true;
                    }
                    break;

                case "BACK": // Delete
                    {
                        txtResult.Focus(); // set the textbox focused.

                        if (txtResult.SelectionStart >= 0) //check if there is text to delete
                        {
                            int TxTindex = txtResult.SelectionStart; // save the index position

                            if (txtResult.SelectedText.Length > 0)  //// check if there is selected text
                            {
                                txtResult.SelectedText = "";
                            }
                            else if (TxTindex > 0) // check if there is text in texbox
                            {
                                Result = Result.Remove(TxTindex - 1, 1);
                                txtResult.SelectionStart = TxTindex - 1; // to set the cursor position after the deleted number between the text.
                            }
                        }
                    }                   
                    break;

                case "CLEAR": // Clear
                    Result = "";
                    break;
                default:

                    {
                        txtResult.Focus();
                        if (isOverDigits(txtResult.Text)) break;
                        int TxTindex1 = txtResult.SelectionStart;
                        string subString1 = txtResult.Text.Substring(0, TxTindex1);
                        string subString2 = txtResult.Text.Substring(TxTindex1, txtResult.Text.Length - subString1.Length);
                        string combinedString = subString1 + button.Content.ToString() + subString2;
                        Result = combinedString;
                        txtResult.SelectionStart = TxTindex1 + 1;
                    }
                    break;
            }
            btnDecimal.IsEnabled = Result.Contains(".") ? false : true;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));

        }

        #endregion
    }
}
