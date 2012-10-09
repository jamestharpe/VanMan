using System;

namespace VanMan.Core
{
    [Flags]
    public enum RedirectOptions
    {
        Default = 0,
        PreservePath = 1, 
        PreserveQueryString = 2,
        //PreservePathAndQuery = PreservePath | PreserveQueryString,
        Permanent = 4
    }
}
