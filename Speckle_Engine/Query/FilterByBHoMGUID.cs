using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        public static IEnumerable<IBHoMObject> FilterByBHoMGUID(IEnumerable<IBHoMObject> objects, List<string> bhomGuid)
        {
            return objects.Where(o => bhomGuid.Any(id => id == o.BHoM_Guid.ToString())); //Inefficient o(n*m) -- implement some kind of sorting https://stackoverflow.com/a/25184658/3873799
        }
    }
}
