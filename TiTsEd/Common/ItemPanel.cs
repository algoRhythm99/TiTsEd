using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TiTsEd.Common
{
    public class ItemPanel : Panel
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(ItemPanel), new PropertyMetadata(0));
        public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(int), typeof(ItemPanel), new PropertyMetadata(200));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public int ColumnWidth
        {
            get { return (int)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        int _rows;
        int _columns;
        int _columnWidth;
        double[] _heights;
        protected override Size MeasureOverride(Size availableSize)
        {
            if (0 == Columns)
            {
                _columnWidth = ColumnWidth;
                _columns = (int)(availableSize.Width / ColumnWidth);
            }
            else
            {
                _columns = Columns;
                _columnWidth = (int)(availableSize.Width / _columns);
            }
            if (0 == _columns)
            {
                _columns = 1;
            }
            _rows = (Children.Count + _columns - 1) / _columns;

            // measure all children
            var itemAvailableSize = new Size(_columnWidth, availableSize.Height);
            foreach (UIElement child in Children) child.Measure(itemAvailableSize);

            _heights = new double[_rows];
            for (int i = 0; i < _heights.Length; i++)
            {
                _heights[i] = GetRow(i).MaxOrZero(x => x.DesiredSize.Height);
            }

            var height = _heights.Sum();
            return new Size(availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double padding = 0.0;
            if (_columns > 1) padding = Math.Floor((finalSize.Width - _columns * _columnWidth) / (_columns - 1));

            double y = 0;
            for (int i = 0; i < _rows; i++)
            {
                double x = 0;
                foreach (var child in GetRow(i))
                {
                    child.Arrange(new Rect(x, y, _columnWidth, _heights[i]));
                    x += _columnWidth + padding;
                }
                y += _heights[i];
            }
            return new Size(finalSize.Width, y);
        }

        IEnumerable<UIElement> GetRow(int row)
        {
            for (int i = row; i < Children.Count; i += _rows)
            {
                yield return Children[i];
            }
        }

        IEnumerable<UIElement> GetColumn(int column)
        {
            for (int i = 0; i < _rows; i++)
            {
                int index = column * _rows + i;
                if (index >= Children.Count) yield break;
                yield return Children[index];
            }
        }
    }
}
