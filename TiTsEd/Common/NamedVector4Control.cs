using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TiTsEd.Common
{
    [TemplatePart(Name = "button", Type = typeof(ToggleButton))]
    public sealed class NamedVector4Control : Control
    {
        public static readonly DependencyProperty IsRightSidedProperty = DependencyProperty.Register("IsRightSided", typeof(bool), typeof(NamedVector4Control), new PropertyMetadata(false));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(NamedVector4Control), new PropertyMetadata(false));

        static NamedVector4Control()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NamedVector4Control), new FrameworkPropertyMetadata(typeof(NamedVector4Control)));
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public bool IsRightSided
        {
            get { return (bool)GetValue(IsRightSidedProperty); }
            set { SetValue(IsRightSidedProperty, value); }
        }

        ToggleButton _button;
        public override void OnApplyTemplate()
        {
            if (_button != null) _button.Checked -= button_Checked;

            _button = GetTemplateChild("button") as ToggleButton;

            if (_button != null) _button.Checked += button_Checked;
        }

        void button_Checked(Object sender, RoutedEventArgs e)
        {
            var window = App.Current.MainWindow as MainWindow;
            var popup = window.ValuesPopup;
            popup.Enable(_button);
        }
    }


    [TemplatePart(Name = "popup", Type = typeof(Popup))]
    public sealed class NamedVector4Popup : Control
    {
        static NamedVector4Popup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NamedVector4Popup), new FrameworkPropertyMetadata(typeof(NamedVector4Popup)));
        }

        Popup _popup;
        public override void OnApplyTemplate()
        {
            if (_popup != null) _popup.Closed -= popup_Closed;

            _popup = GetTemplateChild("popup") as Popup;

            if (_popup != null) _popup.Closed += popup_Closed;
        }

        ToggleButton _button;
        public void Enable(ToggleButton button)
        {
            if (_button == button) return;

            if (_button != null)
            {
                _button.IsChecked = false;
                _button.IsEnabled = true;
                _button = null;
            }

            if (button != null)
            {
                _button = button;
                _button.IsEnabled = false;
                _button.IsChecked = true;
                _popup.DataContext = _button.DataContext;
            }

            _popup.PlacementTarget = button;
            _popup.IsOpen = (button != null);
        }

        void popup_Closed(object sender, EventArgs e)
        {
            Enable(null);
        }
    }
}
