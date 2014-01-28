using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class IntPtrExtensions
{
    // these functions exist in new versions of .NET.
    // So, we add them as extensions to IntPtr until Unity updates the .NET Framework version.

    public static IntPtr Increment(this IntPtr ptr, int cbSize)
    {
        return new IntPtr(ptr.ToInt64() + cbSize);
    }

    public static IntPtr Increment<T>(this IntPtr ptr)
    {
        return Increment(ptr, Marshal.SizeOf(typeof(T)));
    }

    public static T ElementAt<T>(this IntPtr ptr, int index)
    {
        var offset = Marshal.SizeOf(typeof(T)) * index;
        var offsetPtr = ptr.Increment(offset);
        return (T)Marshal.PtrToStructure(offsetPtr, typeof(T));
    }
}
