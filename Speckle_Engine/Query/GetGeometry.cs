using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Geometry;

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        [Description("")]
        [Output("")]
        public static IGeometry GetGeometry(this IBHoMObject bHoMObject, Dictionary<Type, MethodInfo> getGeometryMethods)
        {
            Type BHoMType = bHoMObject.GetType();
            MethodInfo getGeometryMethod = null;
            IGeometry geom = null;

            if (getGeometryMethods.TryGetValue(BHoMType, out getGeometryMethod))
                geom = (IGeometry)getGeometryMethod.Invoke(bHoMObject, new object[] { bHoMObject }); // invokes the extension method `Geometry`
            else
                BH.Engine.Reflection.Compute.RecordError($"Could not find the geometry representation for the type {BHoMType.Name}.");

            return geom;
        }
    }
}
