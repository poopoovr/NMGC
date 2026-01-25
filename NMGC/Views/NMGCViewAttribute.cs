using System;

namespace NMGC.Views;

[AttributeUsage(AttributeTargets.Class)]
public class NMGCViewAttribute(string viewName) : Attribute
{
    public readonly string ViewName = viewName;
}