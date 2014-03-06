﻿using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <remarks>Uses yield return/break.</remarks>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source,
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
                yield return source.Chunk(chunkSize, start, totalCount);
            }
        }
        /// <summary>
        /// Yield the next chunkSize elements starting at start and break if no more elements left.
        /// </summary>
        private static IEnumerable<T> Chunk<T>(this IEnumerable<T> source,
            int chunkSize, int start, int totalCount)
        {
            source.ThrowIfArgumentNull("source");
            chunkSize.ThrowIfArgumentOutOfRange("chunkSize");
            for (int i = 0; i < chunkSize; i++)
            {
                if (start + i == totalCount)
                {
                    yield break;
                }
                yield return source.ElementAt(start + i);
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
            where T : IComparable
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
            if (emptyCheck)
            {
                if (source.Count() == 0)
                {
                    return null;
                }
            }
            if (source.Count() == count)
            {
                return source;
            }
            throw new InvalidOperationException(
                string.Format("The input sequence does not contain {0} elements.", count)
                );
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
    }
}
