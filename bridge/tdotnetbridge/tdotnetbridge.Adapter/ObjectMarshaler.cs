/***************************************************************************************************
 Copyright (C) 2023 The Qt Company Ltd.
 SPDX-License-Identifier: LicenseRef-Qt-Commercial OR LGPL-3.0-only OR GPL-2.0-only OR GPL-3.0-only
***************************************************************************************************/

using System.Runtime.InteropServices;

namespace tdotnetbridge.Adapter;

/// <summary>
/// Custom interop marshaling of object references
/// </summary>
internal class ObjectMarshaler : ICustomMarshaler
{
    private bool _useWeakRefs;
    private static ObjectMarshaler NormalRefMarshaler { get; } = new() { _useWeakRefs = false };
    private static ObjectMarshaler WeakRefMarshaler { get; } = new() { _useWeakRefs = true };

    public static ICustomMarshaler GetInstance(string refMode)
    {
        return refMode == "weak" ? WeakRefMarshaler : NormalRefMarshaler;
    }

    public int GetNativeDataSize()
    {
        return Marshal.SizeOf(typeof(IntPtr));
    }

    public IntPtr MarshalManagedToNative(object? obj)
    {
        if (obj == null)
            return IntPtr.Zero;
        return Adapter.GetRefPtrToObject(obj, _useWeakRefs);
    }

    public object MarshalNativeToManaged(IntPtr objRefPtr)
    {
        if (objRefPtr == IntPtr.Zero)
            return null!;
        var objRef = Adapter.GetObjectRefFromPtr(objRefPtr);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (objRef != null)
            return objRef.Target;
        if (Marshal.PtrToStringUni(objRefPtr) is { } str)
            return str;
        throw new ArgumentException("Invalid object reference", nameof(objRefPtr));
    }

    public void CleanUpManagedData(object obj)
    {
    }

    public void CleanUpNativeData(IntPtr pId)
    {
    }
}