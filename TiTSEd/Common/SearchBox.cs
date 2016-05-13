using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TiTsEd.Common
{
    [TemplatePart(Name = "prompt", Type = typeof(TextBlock))]
    public sealed class SearchBox : Control
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        TextBlock _prompt;
        public override void OnApplyTemplate()
        {
            _prompt = GetTemplateChild("prompt") as TextBlock;
            OnTextChanged(this, new DependencyPropertyChangedEventArgs());
        }

        static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var box = obj as SearchBox;
            if (box._prompt == null) return;
            box._prompt.Visibility = (String.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Collapsed);
        }

    }
}
