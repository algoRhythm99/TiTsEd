using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TiTsEd.Common
{
    [TemplatePart(Name = "popup", Type = typeof(Popup))]
    [TemplatePart(Name = "combo", Type = typeof(ComboBox))]
    [TemplatePart(Name = "nameCombo", Type = typeof(ToggleButton))]
    public sealed class MyPiercingBox : Control
    {
        static MyPiercingBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyPiercingBox), new FrameworkPropertyMetadata(typeof(MyPiercingBox)));
        }

        Popup _popup;
        ComboBox _combo;
        ToggleButton _button;
        MyComboBox _typeCombo;

        public override void OnApplyTemplate()
        {
            if (_popup != null) _popup.Closed -= popup_Closed;
            if (_button != null) _button.Checked -= button_Checked;

            _popup = GetTemplateChild("popup") as Popup;
            _button = GetTemplateChild("button") as ToggleButton;
            _combo = GetTemplateChild("nameCombo") as ComboBox;
            _typeCombo = GetTemplateChild("typeCombo") as MyComboBox;

            if (_button != null) _button.Checked += button_Checked;
            if (_popup != null) _popup.Closed += popup_Closed;
            if (_combo != null) _combo.PreviewKeyDown += _combo_PreviewKeyDown;

            // Circumvent a bug in WPF: this binding is already declared in XAML but WPF removes it for piercing boxes that are not loaded at the start of the application.
            _typeCombo.SetBinding(MyComboBox.SelectedValueProperty, "Type"); 
        }

        void _combo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _combo.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            _button.IsChecked = false;
            _button.IsEnabled = true;
        }

        void button_Checked(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = true;
            _button.IsEnabled = false;
        }
    }
}
