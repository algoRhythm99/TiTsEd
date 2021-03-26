using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel
{
    public enum ListOperations
    {
        Substitution,
        Insertion,
        Deletion
    }

    public struct ListDifference
    {
        public ListOperations Operation;
        public int OldIndex;
        public int NewIndex;
        public int Count;
    }

    public static class UpdatableCollectionHelper
    {
        public static void UpdateFrom<TSource, TTarget>(this List<TTarget> target, IList<TSource> source, Func<TSource, TTarget> selector, IEnumerable<ListDifference> diffs)
        {
            foreach (var diff in diffs)
            {
                if (diff.Operation == ListOperations.Insertion)
                {
                    var items = source.SelectRange(diff.NewIndex, diff.Count).Select(selector);
                    target.InsertRange(diff.NewIndex, items);
                }
                else if (diff.Operation == ListOperations.Deletion)
                {
                    target.RemoveRange(diff.NewIndex, diff.Count);
                }
                else
                {
                    var items = source.SelectRange(diff.NewIndex, diff.Count).Select(selector);
                    target.RemoveRange(diff.NewIndex, diff.Count);
                    target.InsertRange(diff.NewIndex, items);
                }
            }
        }

        public static List<ListDifference> Diff<TSource, TTarget>(this IList<TSource> source, IList<TTarget> target, Func<TSource, TTarget, bool> equalityPredicate)
        {
            var diffs = new List<ListDifference>();
            int substitutionLength = 0;
            int oldIndex = 0;
            int newIndex = 0;

            // Compare one by one
            while (oldIndex < source.Count && newIndex < target.Count)
            {
                var oldItem = source[oldIndex];
                var newItem = target[newIndex];

                // Both elements are the same
                if (equalityPredicate(oldItem, newItem))
                {
                    if (substitutionLength > 0)
                    {
                        diffs.Add(new ListDifference { Operation = ListOperations.Substitution, NewIndex = newIndex - substitutionLength, OldIndex = oldIndex - substitutionLength, Count = substitutionLength });
                        substitutionLength = 0;
                    }

                    ++oldIndex;
                    ++newIndex;
                    continue;
                }

                // Was it an insertion? Then the old item is further in the new collection
                // Ex: abcdef -> abcXYZdef (oldItem = 'd' ; newItem = 'X')
                int matchIndex = target.IndexOf(x => equalityPredicate(oldItem, x), newIndex + 1);
                if (matchIndex != -1)
                {
                    if (substitutionLength > 0)
                    {
                        diffs.Add(new ListDifference { Operation = ListOperations.Substitution, NewIndex = newIndex - substitutionLength, OldIndex = oldIndex - substitutionLength, Count = substitutionLength });
                        substitutionLength = 0;
                    }

                    diffs.Add(new ListDifference { Operation = ListOperations.Insertion, NewIndex = newIndex, OldIndex = oldIndex, Count = matchIndex - newIndex });
                    newIndex = matchIndex + 1;
                    ++oldIndex;
                    continue;
                }

                // Was it a deletion? Then the new item is further in the old collection
                // Ex: abCDef -> abef (oldItem = 'c' ; newItem = 'e')
                matchIndex = source.IndexOf(x => equalityPredicate(x, newItem), oldIndex + 1);
                if (matchIndex != -1)
                {
                    if (substitutionLength > 0)
                    {
                        diffs.Add(new ListDifference { Operation = ListOperations.Substitution, NewIndex = newIndex - substitutionLength, OldIndex = oldIndex - substitutionLength, Count = substitutionLength });
                        substitutionLength = 0;
                    }

                    diffs.Add(new ListDifference { Operation = ListOperations.Deletion, NewIndex = newIndex, OldIndex = oldIndex, Count = matchIndex - oldIndex });
                    oldIndex = matchIndex + 1;
                    ++newIndex;
                    continue;
                }

                // So it was a substitution.
                // Ex: abCDef -> abXYZdef (oldItem  / new ; c / X)
                ++substitutionLength;
                ++oldIndex;
                ++newIndex;
            }

            // Substitution left undealt with
            if (substitutionLength > 0)
            {
                diffs.Add(new ListDifference { Operation = ListOperations.Substitution, NewIndex = newIndex - substitutionLength, OldIndex = oldIndex - substitutionLength, Count = substitutionLength });
            }

            // Deleted items at the end
            if (oldIndex < source.Count)
            {
                diffs.Add(new ListDifference { Operation = ListOperations.Deletion, NewIndex = newIndex, OldIndex = oldIndex, Count = source.Count - oldIndex });
            }

            // Inserted items at then end
            if (newIndex < target.Count)
            {
                diffs.Add(new ListDifference { Operation = ListOperations.Insertion, NewIndex = newIndex, OldIndex = oldIndex, Count = target.Count - newIndex });
            }

            return diffs;
        }

        public static int IndexOf<T>(this IList<T> items, Func<T, bool> predicate, int start = 0)
        {
            for (int i = start; i < items.Count; i++)
            {
                if (predicate(items[i])) return i;
            }
            return -1;
        }

        public static IEnumerable<T> SelectRange<T>(this IList<T> list, int start, int count)
        {
            int last = start + count - 1;
            for (int i = start; i <= last; i++)
            {
                yield return list[i];
            }
        }
    }
}
