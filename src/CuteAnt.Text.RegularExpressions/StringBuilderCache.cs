﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StringBuilderCache
**
** Purpose: provide a cached reusable instance of stringbuilder
**          per thread  it's an optimisation that reduces the 
**          number of instances constructed and collected.
**
**  Acquire - is used to get a string builder to use of a 
**            particular size.  It can be called any number of 
**            times, if a stringbuilder is in the cache then
**            it will be returned and the cache emptied.
**            subsequent calls will return a new stringbuilder.
**
**            A StringBuilder instance is cached in 
**            Thread Local Storage and so there is one per thread
**
**  Release - Place the specified builder in the cache if it is 
**            not too big.
**            The stringbuilder should not be used after it has 
**            been released.
**            Unbalanced Releases are perfectly acceptable.  It
**            will merely cause the runtime to create a new 
**            stringbuilder next time Acquire is called.
**
**  GetStringAndRelease
**          - ToString() the stringbuilder, Release it to the 
**            cache and return the resulting string
**
===========================================================*/
using System;
using System.Text;
using System.Threading;

namespace CuteAnt.Text
{
  /// <summary>StringBuilderCache</summary>
  public static class StringBuilderCache
  {
    // The value 360 was chosen in discussion with performance experts as a compromise between using
    // as litle memory (per thread) as possible and still covering a large part of short-lived
    // StringBuilder creations on the startup path of VS designers.
    private const int MAX_BUILDER_SIZE = 1024 * 4; // 360;
    private const int DefaultCapacity = 16;

    [ThreadStatic]
    private static StringBuilder CachedInstance;

    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
      if (capacity <= MAX_BUILDER_SIZE)
      {
        StringBuilder sb = StringBuilderCache.CachedInstance;
        if (sb != null)
        {
          // Avoid stringbuilder block fragmentation by getting a new StringBuilder
          // when the requested size is larger than the current capacity
          if (capacity <= sb.Capacity)
          {
            StringBuilderCache.CachedInstance = null;
#if NET35 || NET30 || NET20
              sb.Remove(0, sb.Length);
#else
              sb.Clear();
#endif
            return sb;
          }
        }
      }
      return new StringBuilder(capacity);
    }

    public static void Release(StringBuilder sb)
    {
      if (sb.Capacity <= MAX_BUILDER_SIZE)
      {
        StringBuilderCache.CachedInstance = sb;
      }
    }

    public static string GetStringAndRelease(StringBuilder sb)
    {
      string result = sb.ToString();
      Release(sb);
      return result;
    }
  }
}
