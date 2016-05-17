using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TiTsEd.Common
{
    public static class Extensions
    {
        public static IEnumerable<UIElement> AncestorsAndSelf(this UIElement element)
        {
            while (element != null)
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
        }

        public static double MaxOrZero<T>(this IEnumerable<T> items, Func<T, double> selector)
        {
            double value = 0.0;
            foreach (var item in items) value = Math.Max(value, selector(item));
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue existingValue;
            if (dictionary.TryGetValue(key, out existingValue)) return existingValue;
            return defaultValue;
        }

    }


}
