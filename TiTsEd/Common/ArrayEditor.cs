using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiTsEd.ViewModel;

namespace TiTsEd.Common
{
    public class ArrayInsertEventArgs : EventArgs
    {
        public Object Object { get; set; }
    }
    public delegate void ArrayInsertEventHandler(DependencyObject sender, ArrayInsertEventArgs e);


    [TemplatePart(Name = "contentBorder", Type = typeof(Border))]
    [TemplatePart(Name = "removeButton", Type = typeof(Button))]
    [TemplatePart(Name = "addButton", Type = typeof(Button))]
    [TemplatePart(Name = "listBox", Type = typeof(ListBox))]
    public class ArrayEditor : ContentControl
    {
        public static readonly RoutedEvent CreateEvent = EventManager.RegisterRoutedEvent("Create", RoutingStrategy.Direct, typeof(ArrayInsertEventHandler), typeof(ArrayEditor));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IArrayVM), typeof(ArrayEditor), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ArrayEditor), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(ArrayEditor), new PropertyMetadata("", OnPropertiesChanged));
        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register("Capacity", typeof(int), typeof(ArrayEditor), new PropertyMetadata(Int32.MaxValue, OnPropertiesChanged));
        public static readonly DependencyProperty MinCapacityProperty = DependencyProperty.Register("MinCapacity", typeof(int), typeof(ArrayEditor), new PropertyMetadata(0, OnPropertiesChanged));
        public static readonly DependencyProperty SelectedItemObjectProperty = DependencyProperty.Register("SelectedItemObject", typeof(object), typeof(ArrayEditor), new PropertyMetadata(null, OnPropertiesChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ArrayEditor), new PropertyMetadata(-1, OnPropertiesChanged));
        public static readonly DependencyProperty AllowReorderProperty = DependencyProperty.Register("AllowReorder", typeof(bool), typeof(ArrayEditor), new PropertyMetadata(true, OnPropertiesChanged));

        static ArrayEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArrayEditor), new FrameworkPropertyMetadata(typeof(ArrayEditor)));
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public IArrayVM Items
        {
            get { return (IArrayVM)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public bool AllowReorder
        {
            get { return (bool)GetValue(AllowReorderProperty); }
            set { SetValue(AllowReorderProperty, value); }
        }

        public int Capacity
        {
            get { return (int)GetValue(CapacityProperty); }
            set { SetValue(CapacityProperty, value); }
        }

        public int MinCapacity
        {
            get { return (int)GetValue(MinCapacityProperty); }
            set { SetValue(MinCapacityProperty, value); }
        }

        public object SelectedItemObject
        {
            get
            {
                return (null != _listBox) ? Items[_listBox.SelectedIndex] : null;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return (null != _listBox) ? _listBox.SelectedIndex : -1;
            }
            set
            {
                _listBox.SelectedIndex = value;
            }
        }

        ListBox _listBox;
        Button _addButton;
        Button _removeButton;
        Border _contentBorder;
        public override void OnApplyTemplate()
        {
            if (AllowReorder)
            {
                if (_listBox != null) _listBox.DragOver -= listBox_DragOver;
                if (_listBox != null) _listBox.DragEnter -= listBox_DragOver;
                if (_listBox != null) _listBox.PreviewMouseMove -= listBox_PreviewMouseMove;
                if (_listBox != null) _listBox.PreviewMouseLeftButtonDown -= listBox_PreviewMouseLeftButtonDown;
            }
            else
            {
                if (_listBox != null)
                {
                    _listBox.AllowDrop = false;
                    _listBox.ToolTip = null;
                }
            }
            if (_listBox != null) _listBox.SelectionChanged -= listBox_SelectionChanged;
            if (_removeButton != null) _removeButton.Click -= removeButton_Click;
            if (_addButton != null) _addButton.Click -= addButton_Click;

            _listBox = GetTemplateChild("listBox") as ListBox;
            _addButton = GetTemplateChild("addButton") as Button;
            _removeButton = GetTemplateChild("removeButton") as Button;
            _contentBorder = GetTemplateChild("contentBorder") as Border;

            if (AllowReorder)
            {
                if (_listBox != null) _listBox.DragOver += listBox_DragOver;
                if (_listBox != null) _listBox.DragEnter += listBox_DragOver;
                if (_listBox != null) _listBox.PreviewMouseMove += listBox_PreviewMouseMove;
                if (_listBox != null) _listBox.PreviewMouseLeftButtonDown += listBox_PreviewMouseLeftButtonDown;
            }
            else
            {
                if (_listBox != null)
                {
                    _listBox.AllowDrop = false;
                    _listBox.ToolTip = null;
                }
            }
            if (_listBox != null) _listBox.SelectionChanged += listBox_SelectionChanged;
            if (_removeButton != null) _removeButton.Click += removeButton_Click;
            if (_addButton != null) _addButton.Click += addButton_Click;

            OnContentChanged();
        }

        Type _draggedType;
        Point _dragSourcePoint;
        void listBox_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedType != null && e.Data.GetDataPresent(_draggedType))
            {
                e.Effects = DragDropEffects.Move;
                var data = MoveDraggedItem(e);
                if (data != null) e.Data.SetData(data); // The VM changed after the update
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        Object MoveDraggedItem(DragEventArgs e)
        {
            var targetItem = ((UIElement)e.OriginalSource).AncestorsAndSelf().FirstOrDefault(x => x is ListBoxItem) as ListBoxItem;
            int targetIndex = targetItem != null ? Items.IndexOf(targetItem.DataContext) : Items.Count - 1;

            return MoveDraggedItem(e.Data, targetIndex);
        }

        Object MoveDraggedItem(IDataObject dataObject, int targetIndex)
        {
            if (_draggedType == null) return null;

            var droppedData = dataObject.GetData(_draggedType);

            if (droppedData == null) return null;

            int sourceIndex = Items.IndexOf(droppedData);

            if (sourceIndex == targetIndex) return null;

            Items.MoveItemToIndex(sourceIndex, targetIndex);
            _listBox.SelectedIndex = targetIndex;

            return Items[targetIndex];
        }

        void listBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // M1 not pressed?
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // Not engouh distance from start point?
            var diff = e.GetPosition(this) - _dragSourcePoint;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            // Get dragged item
            var draggedItem = ((UIElement)e.OriginalSource).AncestorsAndSelf().FirstOrDefault(x => x is ListBoxItem) as ListBoxItem;
            if (draggedItem == null) return;

            // Do drag drop
            _draggedType = draggedItem.DataContext.GetType();
            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            draggedItem.IsSelected = true;
        }

        void listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragSourcePoint = e.GetPosition(this);
        }

        void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (Items.Count == Capacity) return;

            Items.Create();
            _listBox.SelectedIndex = Items.Count - 1;

            OnContentChanged();
        }

        void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Items.Count == MinCapacity) return;

            int index = _listBox.SelectedIndex;
            _listBox.SelectedIndex = Math.Min(index + 1, Items.Count - 2);
            Items.Delete(index);

            OnContentChanged();
        }

        static void OnPropertiesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ArrayEditor box = (ArrayEditor)obj;

            if (box._listBox == null) return;

            box.OnContentChanged();
        }

        void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnContentChanged();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ((FrameworkElement)Content).DataContext = null;

            OnContentChanged();
        }

        void OnContentChanged()
        {
            if (_listBox == null) return;

            _addButton.IsEnabled = (Items != null) && (Items.Count < Capacity);
            _removeButton.IsEnabled = (Items != null) && (Items.Count > MinCapacity);

            var item = _listBox.SelectedItem;
            if (null == item)
            {
                if (null != _listBox.Items)
                {
                    if (_listBox.Items.Count > 0)
                    {
                        _listBox.SelectedIndex = 0;
                        _listBox.SelectedItem = _listBox.Items.GetItemAt(0);
                        item = _listBox.SelectedItem;
                    }
                }
            }

            if (item != null)
            {
                ((FrameworkElement)Content).DataContext = item;
                _contentBorder.Visibility = Visibility.Visible;
                return;
            }

            if ((Items == null) || (Items.Count == MinCapacity))
            {
                ((FrameworkElement)Content).DataContext = null;
                _contentBorder.Visibility = Visibility.Collapsed;
                return;
            }

            if (_listBox.SelectedIndex == -1)
            {
                _listBox.SelectedIndex = 0;
            }
        }
    }
}
