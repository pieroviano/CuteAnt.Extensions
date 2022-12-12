#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kaos.Collections;

namespace System.Collections
{
    public class SortedSet<T> : RankedSet<T>
    {

        public SortedSet(IEnumerable<T> collection, IComparer<T> comparer)
            : base(collection, comparer)
        {
        }
    }
}
#endif