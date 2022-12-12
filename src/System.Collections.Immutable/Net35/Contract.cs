using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#if NET35
public class Contract
{
    public static void Ensures(object o)
    {
    }

    public static T Result<T>()
    {
        return default;
    }

    public static void Assume(bool isEmpty)
    {
    }

    public static void EnsuresOnThrow<T>(bool b)
    {
    }
}

#endif