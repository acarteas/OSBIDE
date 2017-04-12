using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library
{
    /// <summary>
    /// Disjoint set class.  TODO: replace Tuple container with something more efficient.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DisjointSet<T> where T : IComparable<T>
    {
        private Dictionary<T, Tuple<int, T>> _sets;

        public DisjointSet()
        {
            _sets = new Dictionary<T, Tuple<int, T>>();
        }

        /// <summary>
        /// Returns all current sets as a list 
        /// </summary>
        /// <returns></returns>
        public List<List<T>> AllSets()
        {
            Dictionary<T, List<T>> sets = new Dictionary<T, List<T>>();
            foreach(var item in _sets)
            {
                if(sets.ContainsKey(item.Value.Item2) == false)
                {
                    sets.Add(item.Value.Item2, new List<T>());
                }
                sets[item.Value.Item2].Add(item.Key);
            }
            return sets.Values.ToList();
        }

        /// <summary>
        /// Performs a union between two sets
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void UnionWith(T first, T second)
        {
            //find roots of both sets
            T firstRoot = Find(first);
            T secondRoot = Find(second);

            //if the items aren't already in the set, perform a union
            if (firstRoot.CompareTo(secondRoot) != 0)
            {
                var firstSet = _sets[firstRoot];
                var secondSet = _sets[secondRoot];

                //error checking: make sure that both items are indeed root of their
                //respective sets
                if (firstSet.Item1 < 0 && secondSet.Item1 < 0)
                {
                    //start with the assumption that "first" is larger
                    var largest = firstSet;
                    var smallest = secondSet;
                    int firstSize = firstSet.Item1;
                    int secondSize = secondSet.Item1;

                    //use abs to get rid of negative value
                    firstSize = Math.Abs(firstSize);
                    secondSize = Math.Abs(secondSize);

                    //if second is indeed larger, perform a swap
                    if (firstSize < secondSize)
                    {
                        largest = secondSet;
                        smallest = firstSet;
                    }

                    //update largest's size
                    _sets[largest.Item2] = new Tuple<int, T>(-(firstSize + secondSize), largest.Item2);
                    largest = _sets[largest.Item2];

                    //make smallest a child of the largest
                    _sets[smallest.Item2] = new Tuple<int, T>(1, largest.Item2);
                }
            }
        }

        /// <summary>
        /// Finds the set to which <paramref name="item"/> belongs.  If not in a set,
        /// will create a new set for the item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public T Find(T item)
        {
            //keeps pointer to "top" item in set
            Tuple<int, T> top = null;

            //bubble up until we find the top of the set
            T toFind = item;

            //allow for the case that we already are at the top of the set
            if(_sets.ContainsKey(toFind))
            {
                top = _sets[toFind];
            }

            //if we are not at the top of the set, find the top
            while (_sets.ContainsKey(toFind) && _sets[toFind].Item1 > 0)
            {
                top = _sets[toFind];
                toFind = _sets[toFind].Item2;
            }

            //does the item not exist in our set?  If so, add
            if (top == null)
            {
                _sets.Add(
                    item,
                    new Tuple<int, T>(-1, item)
                    );
                top = _sets[item];
            }

            //perform path compression
            toFind = item;
            while (_sets.ContainsKey(toFind) && _sets[toFind].Item2.CompareTo(top.Item2) != 0)
            {
                //remap current level so that it points to the true "top" of the set
                Tuple<int, T> current = _sets[toFind];
                T currentItem = current.Item2;
                _sets[toFind] = new Tuple<int, T>(current.Item1, top.Item2);

                //move on to the next level
                toFind = currentItem;
            }

            //finally, return the top-most item
            return top.Item2;
        }
    }
}
