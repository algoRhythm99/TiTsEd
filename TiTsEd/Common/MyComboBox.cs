using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TiTsEd.Model;

namespace TiTsEd.Common
{
    [TemplatePart(Name = "combo", Type = typeof(ComboBox))]
    public class MyComboBox : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MyComboBox), new PropertyMetadata(null, OnItemsSourceChanged));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(Object), typeof(MyComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        readonly ObservableCollection<Object> InternalItems = new ObservableCollection<Object>();

        static MyComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyComboBox), new FrameworkPropertyMetadata(typeof(MyComboBox)));
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            var combo = Template.FindName("combo", this) as ComboBox;
            combo.ItemsSource = InternalItems;
        }


        static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyComboBox box = (MyComboBox)obj;
            box.PopulateInternalItems();
            box.AddUnknownItem();
        }

        static void OnSelectedValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MyComboBox box = (MyComboBox)obj;
            box.AddUnknownItem();
        }

        void PopulateInternalItems()
        {
            if (ItemsSource == null) return;
            var selectedValue = SelectedValue;

            InternalItems.Clear();
            foreach (var item in ItemsSource) InternalItems.Add(item);

            SelectedValue = selectedValue;
        }

        void AddUnknownItem()
        {
            if (ItemsSource == null) return;
            if (SelectedValue == null) return;

            if (SelectedValue is string)
            {
                var value = (string) SelectedValue;
                if (InternalItems.Cast<XmlItem>().Any(x => x.ID == value)) return;

                InternalItems.Add(new XmlItem { ID = value, Name = value });
            }
            else
            {
                var value = (int) SelectedValue;
                if (InternalItems.Cast<XmlEnum>().Any(x => x.ID == value)) return;

                InternalItems.Add(new XmlEnum { ID = value, Name = String.Format("[ID#: {0}] <unknown>", value ) });
            }
        }
    }
}
