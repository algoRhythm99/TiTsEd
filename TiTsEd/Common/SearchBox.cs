using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TiTsEd.Common
{
    [TemplatePart(Name = "prompt", Type = typeof(TextBlock))]
    [TemplatePart(Name = "searchTextBox", Type = typeof(TextBox))]
    public sealed class SearchBox : Control
    {
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public string SearchText
        {
            get { return GetValue(SearchTextProperty) as string; }
            set { SetValue(SearchTextProperty, value); }
        }

        TextBlock _prompt;
        TextBox _searchBox;
        public override void OnApplyTemplate()
        {
            _prompt = GetTemplateChild("prompt") as TextBlock;
            _searchBox = GetTemplateChild("searchTextBox") as TextBox;
            OnSearchTextChanged(this, new DependencyPropertyChangedEventArgs());
        }

        static void OnSearchTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var box = obj as SearchBox;
            if (box._prompt == null) return;
            box._prompt.Visibility = (String.IsNullOrEmpty(box.SearchText) ? Visibility.Visible : Visibility.Collapsed);
        }

    }
}
