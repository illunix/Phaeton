﻿namespace Phaeton.Contexts.Accessors;

public interface IContextAccessor
{
    IContext? Context { get; set; }
}