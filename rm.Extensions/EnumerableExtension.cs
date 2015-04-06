﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rm.Extensions
{
    /// <summary>
    /// IEnumerable extensions.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Split the collection into collections of size chunkSize.
        /// </summary>
        /// <remarks>
        /// Uses yield return. But uses ElementAt(index) which is inefficient.
        /// </remarks>
        [Obsolete]
        internal static IEnumerable<IEnumerable<T>> Chunk_bad1<T>(this IEnumerable<T> source,
            int chunkSize)
        {
            source.ThrowIfArgumentNull("source");
            chunkSize.ThrowIfArgumentOutOfRange("chunkSize");
            // to avoid inefficiency due to Count()
            var totalCount = source.Count();
            for (int start = 0; start < totalCount; start += chunkSize)
            {
                // note: skip/take is slow. not O(n) but (n/chunkSize)^2.
                // yield return source.Skip(chunk).Take(chunkSize);
                yield return source.Chunk_bad1(chunkSize, start, totalCount);
            }
        }
        /// <summary>
        /// Yield the next chunkSize elements starting at start and break if no more elements left.
        /// </summary>
        [Obsolete]
        private static IEnumerable<T> Chunk_bad1<T>(this IEnumerable<T> source,
            int chunkSize, int start, int totalCount)
        {
            source.ThrowIfArgumentNull("source");
            chunkSize.ThrowIfArgumentOutOfRange("chunkSize");
            for (int i = 0; i < chunkSize && start + i < totalCount; i++)
            {
                // note: source.ElementAt(index) is inefficient
                yield return source.ElementAt(start + i);
            }
        }
        /// <summary>
        /// Split the collection into collections of size chunkSize.
        /// </summary>
        /// <remarks>
        /// Uses yield return and enumerator. But does not work with other methods as Count(), ElementAt(index), etc.
        /// </remarks>
        [Obsolete]
        internal static IEnumerable<IEnumerable<T>> Chunk_bad2<T>(this IEnumerable<T> source,
            int chunkSize)
        {
            source.ThrowIfArgumentNull("source");
            chunkSize.ThrowIfArgumentOutOfRange("chunkSize");
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return Chunk_bad2(chunkSize, enumerator);
            }
        }
        /// <summary>
        /// Yield the next chunkSize elements till the enumerator has any.
        /// </summary>
        [Obsolete]
        private static IEnumerable<T> Chunk_bad2<T>(int chunkSize, IEnumerator<T> enumerator)
        {
            var count = 0;
            do
            {
                yield return enumerator.Current;
                count++;
            } while (count < chunkSize && enumerator.MoveNext());
        }
        /// <summary>
        /// Split the collection into collections of size chunkSize.
        /// </summary>
        /// <remarks>
        /// Uses yield return but buffers the chunk before returning. Works with other methods 
        /// as Count(), ElementAt(), etc.
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source,
            int chunkSize)
        {
            source.ThrowIfArgumentNull("source");
            chunkSize.ThrowIfArgumentOutOfRange("chunkSize");
            var count = 0;
            var chunk = new List<T>(chunkSize);
            foreach (var item in source)
            {
                chunk.Add(item);
                count++;
                if (count == chunkSize)
                {
                    yield return chunk.AsEnumerable();
                    chunk = new List<T>(chunkSize);
                    count = 0;
                }
            }
            if (count > 0)
            {
                yield return chunk.AsEnumerable();
            }
        }

        /// <summary>
        /// Returns true if collection is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
        /// <summary>
        /// Split a collection into n parts.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq</remarks>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int parts)
        {
            source.ThrowIfArgumentNull("source");
            parts.ThrowIfArgumentOutOfRange("parts");
            // requires more space for objects
            //var splits_index = source.Select((x, index) => new { x, index = index })
            //    .GroupBy(x => x.index % parts)
            //    .Select(g => g.Select(x => x));
            int i = 0;
            var splits = source.GroupBy(x => i++ % parts)
                .Select(g => g.Select(x => x));
            return splits;
        }
        /// <summary>
        /// Returns true if list is ascendingly or descendingly sorted.
        /// </summary>
        public static bool IsSorted<T>(this IEnumerable<T> source)
            where T : IComparable, IComparable<T>
        {
            source.ThrowIfArgumentNull("source");
            // make an array to avoid inefficiency due to ElementAt(index)
            var sourceArray = source.ToArray();
            if (sourceArray.Length <= 1)
            {
                return true;
            }
            var isSorted = false;
            // asc test
            int i;
            for (i = 1; i < sourceArray.Length; i++)
            {
                if (sourceArray[i - 1].CompareTo(sourceArray[i]) > 0)
                {
                    break;
                }
            }
            isSorted = sourceArray.Length == i;
            if (isSorted)
            {
                return true;
            }
            // desc test
            for (i = 1; i < sourceArray.Length; i++)
            {
                if (sourceArray[i - 1].CompareTo(sourceArray[i]) < 0)
                {
                    break;
                }
            }
            isSorted = sourceArray.Length == i;
            return isSorted;
        }
        /// <summary>
        /// Returns the only two elements of a sequence.
        /// </summary>
        public static IEnumerable<T> Double<T>(this IEnumerable<T> source)
        {
            source.ThrowIfArgumentNull("source");
            return XOrDefaultInternal(source, count: 2, emptyCheck: false);
        }
        /// <summary>
        ///  Returns the only two elements of a sequence that satisfy a specified condition.
        /// </summary>
        public static IEnumerable<T> Double<T>(this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            source.ThrowIfArgumentNull("source");
            predicate.ThrowIfArgumentNull("predicate");
            return Double(source.Where(predicate));
        }
        /// <summary>
        /// Returns the only two elements of a sequence, or a default value if the sequence is empty.
        /// </summary>
        public static IEnumerable<T> DoubleOrDefault<T>(this IEnumerable<T> source)
        {
            source.ThrowIfArgumentNull("source");
            return XOrDefaultInternal(source, count: 2, emptyCheck: true);
        }
        /// <summary>
        /// Returns the only two elements of a sequence that satisfy a specified condition 
        /// or a default value if no such elements exists.
        /// </summary>
        public static IEnumerable<T> DoubleOrDefault<T>(this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            source.ThrowIfArgumentNull("source");
            predicate.ThrowIfArgumentNull("predicate");
            return DoubleOrDefault(source.Where(predicate));
        }
        /// <summary>
        /// Returns the only <paramref name="count"/> elements of a sequence 
        /// or a default value if no such elements exists depending on <paramref name="emptyCheck"/>.
        /// </summary>
        private static IEnumerable<T> XOrDefaultInternal<T>(IEnumerable<T> source,
            int count, bool emptyCheck)
        {
            source.ThrowIfArgumentNull("source");
            count.ThrowIfArgumentOutOfRange("count");
            if (emptyCheck && source.IsNullOrEmpty())
            {
                return null;
            }
            // source.Count() == count is inefficient for large enumerable
            if (source.HasCount(count))
            {
                return source;
            }
            throw new InvalidOperationException(
                string.Format("The input sequence does not contain {0} elements.", count)
                );
        }
        /// <summary>
        /// Returns true if source has exactly <paramref name="count"/> elements efficiently.
        /// </summary>
        /// <remarks>Based on int Enumerable.Count() method.</remarks>
        public static bool HasCount<TSource>(this IEnumerable<TSource> source, int count)
        {
            source.ThrowIfArgumentNull("source");
            var collection = source as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count == count;
            }
            var collection2 = source as ICollection;
            if (collection2 != null)
            {
                return collection2.Count == count;
            }
            int num = 0;
            checked
            {
                using (var enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        num++;
                        if (num > count)
                        {
                            return false;
                        }
                    }
                }
            }
            if (num < count)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Returns true if source has at least <paramref name="count"/> elements efficiently.
        /// </summary>
        /// <remarks>Based on int Enumerable.Count() method.</remarks>
        public static bool HasCountOfAtLeast<TSource>(this IEnumerable<TSource> source, int count)
        {
            source.ThrowIfArgumentNull("source");
            var collection = source as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count >= count;
            }
            var collection2 = source as ICollection;
            if (collection2 != null)
            {
                return collection2.Count >= count;
            }
            int num = 0;
            checked
            {
                using (var enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        num++;
                        if (num >= count)
                        {
                            return true;
                        }
                    }
                }
            }
            // when source has 0 elements
            if (num == count)
            {
                return true;
            }
            return false; // < count
        }
        /// <summary>
        /// Returns a new collection with items shuffled in O(n) time.
        /// </summary>
        /// <remarks>
        /// Fisher-Yates shuffle
        /// http://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle-algorithm
        /// </remarks>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            source.ThrowIfArgumentNull("source");
            rng.ThrowIfArgumentNull("rng");
            var items = source.ToArray();
            for (int i = items.Length - 1; i >= 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                yield return items[swapIndex];
                // no need to swap fully as items[swapIndex] is not used later
                items[swapIndex] = items[i];
            }
        }
        /// <summary>
        /// Returns a new collection with items shuffled in O(n) time.
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return Shuffle(source, new Random());
        }
        /// <summary>
        /// Slice an array as Python.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="start">index to include.</param>
        /// <param name="end">index to exclude.</param>
        /// <param name="step">step</param>
        /// <returns></returns>
        /// <remarks>
        /// http://docs.python.org/2/tutorial/introduction.html#strings
        ///    +---+---+---+---+---+
        ///    | H | e | l | p | A |
        ///    +---+---+---+---+---+
        ///      0   1   2   3   4   5
        /// -6  -5  -4  -3  -2  -1    
        /// 
        /// note:
        /// [1:3] and [-4:-2] give { e, l }
        /// +ve step means traverse forward, -ve step means traverse backward
        /// defaults for +ve step, start = 0, end = 5 (a.Length)
        /// defaults for -ve step, start = -1, end = -6 (-a.Length -1)
        /// </remarks>
        public static IEnumerable<T> Slice<T>(this T[] array,
            int? start = null, int? end = null, int step = 1)
        {
            array.ThrowIfArgumentNull("array");
            int _start, _end;
            // step
            if (step == 0)
            {
                // handle gracefully
                yield break;
            }
            else if (step > 0)
            {
                // defaults for step > 0
                _start = 0;
                _end = array.Length;
            }
            else // step < 0
            {
                // defaults for step < 0
                _start = -1;
                _end = -array.Length - 1;
            }
            // inputs
            _start = start ?? _start;
            _end = end ?? _end;
            // get positive index for given index
            Func<int, int, int> toPositiveIndex = (int index, int length) =>
            {
                return index >= 0 ? index : index + length;
            };
            // start
            if (_start < -array.Length || _start >= array.Length)
            {
                yield break;
            }
            _start = toPositiveIndex(_start, array.Length);
            // end - check gracefully
            if (_end < -array.Length - 1)
            {
                _end = -array.Length - 1;
            }
            if (_end > array.Length)
            {
                _end = array.Length;
            }
            _end = toPositiveIndex(_end, array.Length);
            // start, end
            if (step > 0 && _start > _end)
            {
                yield break;
            }
            if (step < 0 && _end > _start)
            {
                yield break;
            }
            // slice
            for (int i = _start; (step > 0 ? i < _end : i > _end); i += step)
            {
                yield return array[i];
            }
        }
        /// <summary>
        /// Returns a new collection with words scrabbled like the game.
        /// </summary>
        public static IEnumerable<string> Scrabble(this IEnumerable<string> words)
        {
            words.ThrowIfArgumentNull("words");
            var wordsArray = words.Where(x => !x.IsNullOrEmpty()).ToArray();
            // preallocate
            var list = new List<string>(wordsArray.Length.ScrabbleCount());
            Scrabble(wordsArray, new bool[wordsArray.Length], new StringBuilder(), 0, list);
            return list.AsEnumerable();
        }
        /// <summary>
        /// Recursive method to scrabble.
        /// </summary>
        /// <param name="words">Words to scrabble.</param>
        /// <param name="used">Bool array to determine already used word in <paramref name="words"/>.</param>
        /// <param name="buffer">Buffer to hold the words.</param>
        /// <param name="depth">Call depth to determine when to return.</param>
        /// <param name="list">List to hold the scrabbled words.</param>
        /// <remarks>Similar to the permute method.</remarks>
        private static void Scrabble(string[] words, bool[] used, StringBuilder buffer, int depth,
            IList<string> list)
        {
            // add to list here
            if (depth > 0)
            {
                list.Add(buffer.ToString());
            }
            if (depth == words.Length)
            {
                return;
            }
            for (int i = 0; i < words.Length; i++)
            {
                if (used[i])
                {
                    continue;
                }
                used[i] = true;
                buffer.Append(words[i]);
                Scrabble(words, used, buffer, depth + 1, list);
                buffer.Length -= words[i].Length;
                used[i] = false;
            }
        }
        /// <summary>
        /// Convert a collection to HashSet.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static IEnumerable<IEnumerable<T>> Permutation<T>(this IEnumerable<T> source)
        {
            return source.Permutation(source.Count());
        }
        public static IEnumerable<IEnumerable<T>> Permutation<T>(this IEnumerable<T> source, int r)
        {
            source.ThrowIfArgumentNull("source");
            if (!source.HasCountOfAtLeast(r))
            {
                throw new ArgumentOutOfRangeException("r");
            }
            var input = source.ToArray();
            var output = new List<IEnumerable<T>>(input.Length.Permutation(r));
            var buffer = new T[r];
            var flags = new bool[input.Length];
            var depth = 0;
            Permute(input, r, output, buffer, flags, depth);
            return output.AsEnumerable();
        }
        private static void Permute<T>(T[] input, int r, List<IEnumerable<T>> output, T[] buffer,
            bool[] flags, int depth)
        {
            if (depth > r)
            {
                return;
            }
            if (depth == r)
            {
                output.Add((T[])buffer.Clone());
                return;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (flags[i])
                {
                    continue;
                }
                flags[i] = true;
                buffer[depth] = input[i];
                Permute(input, r, output, buffer, flags, depth + 1);
                buffer[depth] = default(T);
                flags[i] = false;
            }
        }
        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> source)
        {
            return source.Combination(source.Count());
        }
        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> source, int r)
        {
            source.ThrowIfArgumentNull("source");
            if (!source.HasCountOfAtLeast(r))
            {
                throw new ArgumentOutOfRangeException("r");
            }
            var input = source.ToArray();
            var output = new List<IEnumerable<T>>(input.Length.Combination(r));
            var buffer = new T[r];
            var flags = new bool[input.Length];
            var depth = 0;
            var start = 0;
            Combine(input, r, output, buffer, flags, depth, start);
            return output.AsEnumerable();
        }
        private static void Combine<T>(T[] input, int r, List<IEnumerable<T>> output, T[] buffer,
            bool[] flags, int depth, int start)
        {
            if (depth > r)
            {
                return;
            }
            if (depth == r)
            {
                output.Add((T[])buffer.Clone());
                return;
            }
            for (int i = start; i < input.Length; i++)
            {
                if (flags[i])
                {
                    continue;
                }
                flags[i] = true;
                buffer[depth] = input[i];
                Combine(input, r, output, buffer, flags, depth + 1, i + 1);
                buffer[depth] = default(T);
                flags[i] = false;
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
        public static bool IsEmpty<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return !source.Any(predicate);
        }

        /// <summary>
        /// Returns top n efficiently.
        /// </summary>
        /// <remarks>Uses min-heap, O(elements * logn) time, O(n) space.</remarks>
        public static IEnumerable<T> Top<T, TKey>(this IEnumerable<T> source, int n, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            source.ThrowIfArgumentNull("source");
            n.ThrowIfArgumentOutOfRange("n");
            keySelector.ThrowIfArgumentNull("keySelector");
            var heap = new T[n];
            if (n == 0)
            {
                return heap.AsEnumerable();
            }
            foreach (var x in source)
            {
                if (x == null)
                {
                    continue;
                }
                if (heap[0] == null //always replace null
                    || (keySelector(x).CompareTo(keySelector(heap[0])) > 0) //x > heap[0]
                    )
                {
                    heap[0] = x;
                    SiftDownMin(heap, 0, keySelector);
                }
            }
            return heap.Where(x => x != null);
        }
        /// <summary>
        /// Returns bottom n efficiently.
        /// </summary>
        /// <remarks>Uses max-heap, O(elements * logn) time, O(n) space.</remarks>
        public static IEnumerable<T> Bottom<T, TKey>(this IEnumerable<T> source, int n, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            source.ThrowIfArgumentNull("source");
            n.ThrowIfArgumentOutOfRange("n");
            keySelector.ThrowIfArgumentNull("keySelector");
            var heap = new T[n];
            if (n == 0)
            {
                return heap.AsEnumerable();
            }
            foreach (var x in source)
            {
                if (x == null)
                {
                    continue;
                }
                if (heap[0] == null //always replace null
                    || (keySelector(x).CompareTo(keySelector(heap[0])) < 0) //x < heap[0]
                    )
                {
                    heap[0] = x;
                    SiftDownMax(heap, 0, keySelector);
                }
            }
            return heap.Where(x => x != null);
        }
        private static void SiftDownMin<T, TKey>(T[] heap, int i, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            var left = 2 * i + 1;
            var right = left + 1;
            var min = i;
            if (left < heap.Length
                && (heap[left] == null //always sift null up
                    || (heap[min] != null //anything < null is false so && and not ||
                        && keySelector(heap[min]).CompareTo(keySelector(heap[left])) > 0) //heap[left] < heap[min]
                    )
                )
            {
                min = left;
            }
            if (right < heap.Length
                && (heap[right] == null
                    || (heap[min] != null
                        && keySelector(heap[min]).CompareTo(keySelector(heap[right])) > 0) //heap[right] < heap[min]
                    )
                )
            {
                min = right;
            }
            if (min != i)
            {
                Helper.Swap(ref heap[min], ref heap[i]);
                SiftDownMin(heap, min, keySelector);
            }
        }
        private static void SiftDownMax<T, TKey>(T[] heap, int i, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            var left = 2 * i + 1;
            var right = left + 1;
            var max = i;
            if (left < heap.Length
                && (heap[left] == null //always sift null up
                    || (heap[max] != null //null < anything is false so && and not ||
                        && keySelector(heap[max]).CompareTo(keySelector(heap[left])) < 0) //heap[max] < heap[left]
                    )
                )
            {
                max = left;
            }
            if (right < heap.Length
                && (heap[right] == null
                    || (heap[max] != null
                        && keySelector(heap[max]).CompareTo(keySelector(heap[right])) < 0) //heap[max] < heap[right]
                    )
                )
            {
                max = right;
            }
            if (max != i)
            {
                Helper.Swap(ref heap[max], ref heap[i]);
                SiftDownMax(heap, max, keySelector);
            }
        }
        /// <summary>
        /// Returns top n efficiently.
        /// </summary>
        public static IEnumerable<T> Top<T>(this IEnumerable<T> source, int n)
            where T : struct, IComparable<T>
        {
            // wrap into reference type, unwrap before returning
            return Top(source.Select(x => new { item = x }), n, x => x.item).Select(x => x.item);
        }
        /// <summary>
        /// Returns top n efficiently.
        /// </summary>
        public static IEnumerable<T> Bottom<T>(this IEnumerable<T> source, int n)
            where T : struct, IComparable<T>
        {
            // wrap into reference type, unwrap before returning
            return Bottom(source.Select(x => new { item = x }), n, x => x.item).Select(x => x.item);
        }
        /// <summary>
        /// Returns source.Except(second, comparer) in a linqified way.
        /// </summary>
        public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> source, IEnumerable<T> second,
            Func<T, TKey> keySelector)
        {
            source.ThrowIfArgumentNull("source");
            second.ThrowIfArgumentNull("second");
            keySelector.ThrowIfArgumentNull("keySelector");
            return source.Except(second,
                // calls new GenericEqualityComparer<T, TKey>(keySelector)
                GenericEqualityComparer<T>.By(keySelector)
                );
        }
        /// <summary>
        /// Returns source.Distinct(comparer) in a linqified way.
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source,
            Func<T, TKey> keySelector)
        {
            source.ThrowIfArgumentNull("source");
            keySelector.ThrowIfArgumentNull("keySelector");
            return source.Distinct(
                // calls new GenericEqualityComparer<T, TKey>(keySelector)
                GenericEqualityComparer<T>.By(keySelector)
                );
        }
        /// <summary>
        /// Returns empty if enumerable is null else same enumerable.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
