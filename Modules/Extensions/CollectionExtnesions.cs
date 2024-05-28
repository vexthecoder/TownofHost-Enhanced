﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOHE
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns a random element from a collection
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <returns>A random element from the collection, or the default value of <typeparamref name="T"/> if the collection is empty</returns>
        public static T RandomElement<T>(this IList<T> collection)
        {
            if (collection.Count == 0) return default;
            return collection[IRandom.Instance.Next(collection.Count)];
        }
        /// <summary>
        /// Combines multiple collections into a single collection
        /// </summary>
        /// <param name="firstCollection">The collection to start with</param>
        /// <param name="collections">The other collections to add to <paramref name="firstCollection"/></param>
        /// <typeparam name="T">The type of the elements in the collections to combine</typeparam>
        /// <returns>A collection containing all elements of <paramref name="firstCollection"/> and all <paramref name="collections"/></returns>
        public static IEnumerable<T> CombineWith<T>(this IEnumerable<T> firstCollection, params IEnumerable<T>[] collections)
        {
            return firstCollection.Concat(collections.SelectMany(x => x));
        }

        /// <summary>
        /// Executes an action for each element in a collection in parallel
        /// </summary>
        /// <param name="collection">The collection to iterate over</param>
        /// <param name="action">The action to execute for each element</param>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        public static void Do<T>(this IEnumerable<T> collection, System.Action<T> action)
        {
            Parallel.ForEach(collection, action);
        }

        /// <summary>
        /// Executes an action for each element in a collection in parallel
        /// </summary>
        /// <param name="collection">The collection to iterate over</param>
        /// <param name="action">The action to execute for each element</param>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        public static void Do<T>(this ParallelQuery<T> collection, System.Action<T> action)
        {
            collection.ForAll(action);
        }

        /// <summary>
        /// Executes an action for each element in a collection in parallel if the predicate is true
        /// </summary>
        /// <param name="collection">The collection to iterate over</param>
        /// <param name="predicate">The predicate to check for each element</param>
        /// <param name="action">The action to execute for each element that satisfies the predicate</param>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        public static void DoIf<T>(this IEnumerable<T> collection, System.Func<T, bool> predicate, System.Action<T> action)
        {
            var partitioner = Partitioner.Create(collection.Where(predicate));
            Parallel.ForEach(partitioner, action);
        }

        /// <summary>
        /// Executes an action for each element in a collection in parallel if the predicate is true
        /// </summary>
        /// <param name="collection">The collection to iterate over</param>
        /// <param name="predicate">The predicate to check for each element</param>
        /// <param name="action">The action to execute for each element that satisfies the predicate</param>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        public static void DoIf<T>(this ParallelQuery<T> collection, System.Func<T, bool> predicate, System.Action<T> action)
        {
            collection.Where(predicate).ForAll(action);
        }

        /// <summary>
        /// Removes an element from a collection
        /// </summary>
        /// <param name="collection">The collection to remove the element from</param>
        /// <param name="element">The element to remove</param>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        /// <returns>A collection containing all elements of <paramref name="collection"/> except for <paramref name="element"/></returns>
        public static ParallelQuery<T> Remove<T>(this IEnumerable<T> collection, T element)
        {
            return collection.AsParallel().Where(x => !x.Equals(element));
        }
    }
}