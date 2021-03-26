using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TiTsEd.Common
{
    public enum NumericType
    {
        Int,
        UInt,
        Double,
    }

    [TemplatePart(Name = "tipText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "textBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "lowerButton", Type = typeof(Button))]
    [TemplatePart(Name = "upperButton", Type = typeof(Button))]
    public class MyNumericBox : Control
    {
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double?), typeof(MyNumericBox), new PropertyMetadata(0.0, OnPropertiesChanged));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double?), typeof(MyNumericBox), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(NumericType), typeof(MyNumericBox), new PropertyMetadata(NumericType.Int, OnPropertiesChanged));

        public static readonly DependencyProperty LowerProperty = DependencyProperty.Register("Lower", typeof(double?), typeof(MyNumericBox), new PropertyMetadata(0.0, OnPropertiesChanged));
        public static readonly DependencyProperty UpperProperty = DependencyProperty.Register("Upper", typeof(double?), typeof(MyNumericBox), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty LowerLabelProperty = DependencyProperty.Register("LowerLabel", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty UpperLabelProperty = DependencyProperty.Register("UpperLabel", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(MyNumericBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPropertiesChanged));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty TipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(MyNumericBox), new PropertyMetadata("", OnPropertiesChanged));

        static readonly DependencyPropertyKey HasErrorPropertyKey = DependencyProperty.RegisterReadOnly("HasError", typeof(bool), typeof(MyNumericBox), new PropertyMetadata(false));
        public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

        static readonly DependencyPropertyKey ErrorTextPropertyKey = DependencyProperty.RegisterReadOnly("ErrorText", typeof(string), typeof(MyNumericBox), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorTextProperty = ErrorTextPropertyKey.DependencyProperty;


        static MyNumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyNumericBox), new FrameworkPropertyMetadata(typeof(MyNumericBox)));
        }

        TextBlock _tipText;
        TextBox _textBox;
        Button _lowerButton;
        Button _upperButton;

        public double? Min
        {
            get { return (double?)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double? Max
        {
            get { return (double?)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public NumericType Type
        {
            get { return (NumericType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public double? Lower
        {
            get { return (double?)GetValue(LowerProperty); }
            set { SetValue(LowerProperty, value); }
        }

        public double? Upper
        {
            get { return (double?)GetValue(UpperProperty); }
            set { SetValue(UpperProperty, value); }
        }

        public string LowerLabel
        {
            get { return GetValue(LowerLabelProperty) as string; }
            set { SetValue(LowerLabelProperty, value); }
        }

        public string UpperLabel
        {
            get { return GetValue(UpperLabelProperty) as string; }
            set { SetValue(UpperLabelProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            private set { SetValue(HasErrorPropertyKey, value); }
        }

        public string ErrorText
        {
            get { return (string)GetValue(ErrorTextProperty); }
            private set { SetValue(ErrorTextPropertyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_lowerButton != null) _lowerButton.Click -= lowerButton_Click;
            if (_upperButton != null) _upperButton.Click -= upperButton_Click;
            if (_textBox != null) _textBox.TextChanged -= textBox_OnTextChanged;
            if (_textBox != null) _textBox.LostKeyboardFocus -= textBox_LostFocus;
            if (_textBox != null) _textBox.GotKeyboardFocus -= textBox_GotKeyboardFocus;
            if (_textBox != null) _textBox.PreviewMouseDown -= _textBox_PreviewMouseDown;
            if (_textBox != null) _textBox.PreviewKeyDown -= textBox_PreviewKeyDown;

            _tipText = GetTemplateChild("tipText") as TextBlock;
            _textBox = GetTemplateChild("textBox") as TextBox;
            _lowerButton = GetTemplateChild("lowerButton") as Button;
            _upperButton = GetTemplateChild("upperButton") as Button;

            if (_lowerButton != null) _lowerButton.Click += lowerButton_Click;
            if (_upperButton != null) _upperButton.Click += upperButton_Click;
            if (_textBox != null) _textBox.TextChanged += textBox_OnTextChanged;
            if (_textBox != null) _textBox.LostKeyboardFocus += textBox_LostFocus;
            if (_textBox != null) _textBox.GotKeyboardFocus += textBox_GotKeyboardFocus;
            if (_textBox != null) _textBox.PreviewMouseDown += _textBox_PreviewMouseDown;
            if (_textBox != null) _textBox.PreviewKeyDown += textBox_PreviewKeyDown;

            Update();
        }

        void textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource != _textBox) return;
            _textBox.SelectAll();
        }

        void _textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_textBox.IsKeyboardFocusWithin) return;
            _textBox.Focus();
            e.Handled = true;
        }

        void textBox_LostFocus(object sender, EventArgs e)
        {
            if (HasError) return;
            _textBox.Text = ValueToText(Value);
        }

        void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                TrySetValue(1 + (int)Value); // 0.5 becomes 1, then 1 becomes 2
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                
                TrySetValue(-(int)(1 - Value)); // -0.5 becomes -1, then -1 becomes -2
                e.Handled = true;
            }
        }

        void lowerButton_Click(object sender, RoutedEventArgs e)
        {
            TrySetValue(Lower.Value);   
        }
        void upperButton_Click(object sender, RoutedEventArgs e)
        {
            TrySetValue(Upper.Value);
        }

        void textBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TrySetValue();
        }

        static void OnPropertiesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyNumericBox box = (MyNumericBox)obj;
            if (box._textBox == null) return;
            box.Update();
        }

        void Update()
        {
            if (!_preserveText) _textBox.Text = ValueToText(Value);
            UpdateButton(_lowerButton, Lower, LowerLabel);
            UpdateButton(_upperButton, Upper, UpperLabel);
            _tipText.Text = GetTip();
        }

        string GetTip()
        {
            if (!String.IsNullOrEmpty(Tip)) return Tip;

            if (Unit == "feet") return String.Format("{0:0}cm", Value * 12 * 2.54);

            if (Unit == "inches")
            {
                if (Value >= 12) return String.Format("{0:0} cm ; {1:0}\u2032 {2:0}\u2033", Value * 2.54, (int)(Value / 12), Value % 12);
                if (Value >= 4) return String.Format("{0:0} cm", Value * 2.54);
                return String.Format("{0:0.0} cm", Value * 2.54);
            }

            return "";
        }

        void UpdateButton(Button button, double? value, string label)
        {
            if (String.IsNullOrEmpty(label) && value.HasValue) label = value.Value.ToString();

            button.Visibility = (value.HasValue && label != "") ? Visibility.Visible : Visibility.Collapsed;
            button.Content = label;

        }

        bool _preserveText;
        bool TrySetValue(bool showError = true)
        {
            double value;
            // Do it now for performances reasons
            if (ValueToText(Value) == _textBox.Text)
            {
                SetError(null);
                return true;
            }

            if (TextToValue(_textBox.Text, out value))
            {
                _preserveText = true;
                try
                {
                    return TrySetValue(value, showError);
                }
                finally
                {
                    _preserveText = false;
                }
            }
            else if (showError) SetError("Invalid format");
            return false;
        }

        bool TrySetValue(double value, bool showError = true)
        {
            string error = GetErrorText(value);
            if (error == null)
            {
                SetError(null);
                Value = value;
                return true;
            }

            if (showError) SetError(error);

            return false;
        }

        string GetErrorText(double value)
        {
            if (Min.HasValue && value < Min.Value) return "Must be " + Min.Value + " or greater.";
            if (Max.HasValue && value > Max.Value) return "Must be " + Max.Value + " or less.";

            if (Type == NumericType.Int)
            {
                const int extremum = 1 << 28; // Flash limit for int29 encoding
                if (value < -extremum) return "Must be " + -extremum + " or greater.";
                if (value >= extremum) return "Must be " + (extremum - 1) + " or less.";
                if (value != (int)value) return "Must be an integer.";
            }
            else if (Type == NumericType.UInt)
            {
                const int extremum = 1 << 29;
                if (value < 0) return "Must be 0 or greater.";
                if (value >= extremum) return "Must be " + (extremum - 1) + " or less.";
                if (value != (int)value) return "Must be an integer.";
            }
            return null;
        }

        void SetError(string text)
        {
            HasError = (text != null);
            ErrorText = text;
        }

        bool TextToValue(string str, out double value)
        {
            value = 0;
            string unit1;
            string unit2;
            double value1;
            double value2;
            if (!Parse(str, out value1, out unit1, out value2, out unit2)) return false;

            var unit = NormalizeUnit(Unit);
            value = value1;

            // No unit was specified?
            if (unit1 == null && unit2 == null) return true;

            // Same unit was specified?
            if (unit1 == unit && unit2 == null) return true;

            //No unit expected?
            if (unit == null) return false;

            // Unit does not match?
            if (unit == "%") return false;

            // Inches expected
            if (unit == "inches")
            {
                if (unit1 == "feet" && unit2 == null)
                {
                    value = 12 * value1;
                    return true;
                }
                if (unit1 == "feet" && unit2 == "inches")
                {
                    value = 12 * value1 + value2;
                    return true;
                }
                if (unit1 == "meters" && unit2 == null)
                {
                    value = value1 / 0.0254;
                    return true;
                }
                if (unit1 == "decimeters" && unit2 == null)
                {
                    value = value1 / 0.254;
                    return true;
                }
                if (unit1 == "centimeters" && unit2 == null)
                {
                    value = value1 / 2.54;
                    return true;
                }
                if (unit1 == "millimeters" && unit2 == null)
                {
                    value = value1 / 25.4;
                    return true;
                }
                return false;
            }

            // Inches expected
            if (unit == "inches²")
            {
                if (unit1 == "feet²" && unit2 == null)
                {
                    value = 12 * 12 * value1;
                    return true;
                }
                if (unit1 == "meters²" && unit2 == null)
                {
                    value = value1 / (0.0254 * 0.0254);
                    return true;
                }
                if (unit1 == "decimeters²" && unit2 == null)
                {
                    value = value1 / (0.254 * 0.254);
                    return true;
                }
                if (unit1 == "centimeters²" && unit2 == null)
                {
                    value = value1 / (2.54 * 2.54);
                    return true;
                }
                if (unit1 == "millimeters" && unit2 == null)
                {
                    value = value1 / (25.4 * 25.4);
                    return true;
                }
                return false;
            }

            return false;
        }

        bool Parse(string str, out double value1, out string unit1, out double value2, out string unit2)
        {
            value1 = 0;
            value2 = 0;
            unit1 = null;
            unit2 = null;

            var separators = @"\.";
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".") separators += CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            var unitFormat = @"[^\d\s" + separators + "]+";
            var numberFormat = @"[+-]?[\d" + separators + "]+";
            var format = @"^\s*(?<value1>" + numberFormat + @")\s*(?<unit1>" + unitFormat + @")?\s*(?<value2>" + numberFormat + @")?\s*(?<unit2>" + unitFormat + @")?\s*$";

            // Match
            var cultureRegex = new Regex(format);
            var match = cultureRegex.Match(str);
            int captures = match.Groups.Cast<Group>().Count(x => x.Success) - 1;
            if (captures == 0) return false;

            // Value 1
            var group = match.Groups["value1"];
            if (!group.Success) return false;
            if (!Double.TryParse(group.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value1) && 
                !Double.TryParse(group.Value, NumberStyles.Float, CultureInfo.CurrentCulture, out value1)) return false;
            if (captures == 1) return true;

            // Unit 1
            group = match.Groups["unit1"];
            if (!group.Success) return false;
            unit1 = NormalizeUnit(group.Value);
            if (captures == 2) return true;

            // Value 2
            group = match.Groups["value2"];
            if (!group.Success) return false;
            if (!Double.TryParse(group.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value2) &&
                !Double.TryParse(group.Value, NumberStyles.Float, CultureInfo.CurrentCulture, out value2)) return false;

            // Unit 1
            group = match.Groups["unit2"];
            if (!group.Success) return false;
            unit2 = NormalizeUnit(group.Value);

            return true;
        }

        static string NormalizeUnit(string unit)
        {
            if (String.IsNullOrEmpty(unit)) return null;

            unit = unit.ToLowerInvariant();
            if (unit.EndsWith("²") && unit.Length > 1) return NormalizeUnit(unit.Substring(0, unit.Length - 1)) + "²";

            switch (unit)
            {
                case "'":
                case "\u2032":
                case "ft":
                case "foot":
                case "feet":
                    return "feet";

                case "\"":
                case "\u2033":
                case "in":
                case "inch":
                case "inches":
                    return "inches";

                case "m":
                case "meter":
                case "meters":
                    return "meters";

                case "dm":
                case "decimeter":
                case "decimeters":
                    return "decimeters";

                case "cm":
                case "centimeter":
                case "centimeters":
                    return "centimeters";

                case "mm":
                case "millimeter":
                case "millimeters":
                    return "millimeters";

                case "d":
                case "day":
                case "days":
                    return "days";

                case "h":
                case "hour":
                case "hours":
                    return "hours";

                default:
                    return unit;
            }
        }

        string ValueToText(double value)
        {
            string str;

            if (Type == NumericType.Double) str = value.ToString("0.0#");
            else str = ((int)value).ToString();

            if (!String.IsNullOrEmpty(Unit)) str += " " + Unit;

            return str;
        }
    }
}
