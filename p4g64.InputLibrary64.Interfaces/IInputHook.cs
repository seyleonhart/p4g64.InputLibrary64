﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4g64.InputLibrary64.Interfaces
{
    // [Flags]
    // public enum Input
    // {
    //     None     = 0x0000,

    //     Select   = 0x0001,
    //     Start    = 0x0008,

    //     Up       = 0x0010,
    //     Right    = 0x0020,
    //     Down     = 0x0040,
    //     Left     = 0x0080,

    //     LB       = 0x0400,
    //     RB       = 0x0800,

    //     Triangle = 0x1000,
    //     Circle   = 0x2000,
    //     Cross    = 0x4000,
    //     Square   = 0x8000
    // }
    
    public interface IInputHook
    {
        event OnInputEvent OnInput;
    }
    public delegate void OnInputEvent(int input, bool risingEdge, bool controlType);
}