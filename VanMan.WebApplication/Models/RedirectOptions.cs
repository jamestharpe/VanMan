using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanMan.WebApplication.App_Code
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
