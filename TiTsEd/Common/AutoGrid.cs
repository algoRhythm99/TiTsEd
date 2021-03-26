using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TiTsEd.Common
{
    public class AutoGrid : Panel
    {
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(AutoGrid), new PropertyMetadata(2));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public int Rows
        {
            get { return (Children.Count + Columns - 1) / Columns; }
        }

        double[] _widthes;
        double[] _heights;
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children) child.Measure(availableSize);

            _widthes = new double[Columns];
            for (int i = 0; i < _widthes.Length; i++)
            {
                _widthes[i] = GetColumn(i).MaxOrZero(x => x.DesiredSize.Width);
            }

            _heights = new double[Rows];
            for (int i = 0; i < _heights.Length; i++)
            {
                _heights[i] = GetRow(i).MaxOrZero(x => x.DesiredSize.Height);
            }

            var width = _widthes.Sum();
            var height = _heights.Sum();
            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            double y = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                int row = i / Columns;
                int column = i % Columns;
                double cellHeight = _heights[row];
                double cellWidth = _widthes[column];

                var child = Children[i];
                child.Arrange(new Rect(x, y, cellWidth, cellHeight));

                if (column == Columns - 1)
                {
                    x = 0;
                    y += cellHeight;
                }
                else
                {
                    x += cellWidth;
                }
            }
            return finalSize;
        }

        IEnumerable<UIElement> GetRow(int row)
        {
            for (int i = 0; i < Columns; i++)
            {
                int index = row * Columns + i;
                if (index >= Children.Count) yield break;
                yield return Children[index];
            }
        }

        IEnumerable<UIElement> GetColumn(int column)
        {
            for (int i = column; i < Children.Count; i += Columns)
            {
                yield return Children[i];
            }
        }
    }
}
