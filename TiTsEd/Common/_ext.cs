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

        public static string ReplaceUsingDictionary(string source, IDictionary<string, object> replacements) {
            if (null == source) return null;
            string replaced = source;
            foreach (var key in replacements.Keys) {
                replaced = replaced.Replace(key, Convert.ToString(replacements[key]));
            }
            return replaced;
        }

        public static string GetFeetAndCentimetersDescription(double inches) {
            const double inchesToCM = 2.54;
            if (inches >= 12) return String.Format("{1:0}\u2032 {2:0}\u2033 ; {0:0.0} cm", inches * inchesToCM, (int)(inches / 12), inches % 12);
            return String.Format("{0:0.0} inches ; {1:0.0} cm", inches, inches * inchesToCM);
        }


        public static string GetCubicInchesOrCentimetersDescription(double volume)
        {
            const double inchesToCM = 16.3871;
            return String.Format("{0:0.00} cubic inches ; {1:0.00} cubic cm", volume, volume * inchesToCM);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsInsensitive(this string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }


}
