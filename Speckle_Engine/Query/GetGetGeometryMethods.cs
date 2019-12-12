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

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        [Description("Dynamically returns the BHoM GetGeometry extension methods avaialble for any BHoMObject, based on the assemblies that have been loaded at runtime.")]
        [Output("Key is the specific BHoMObject type, Value is the `Geometry()` method that returns the geom of the BHoMObject.")]
        public static Dictionary<Type, MethodInfo> GetGetGeometryMethods()
        {
            // To be returned
            Dictionary<Type, MethodInfo> getGeometryMethods = new Dictionary<Type, MethodInfo>();

            // Useful for debug
            List<Assembly> bhomAssemblies = new List<Assembly>();
            List<Assembly> bhomEngineAssemblies = new List<Assembly>();

            // Load all the assemblies, filter out only BHoM ones, then get the `Geometry()` methods.
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            try
            {
                foreach (Assembly a in loadedAssemblies)
                    if (!a.IsDynamic && a.Location.Contains("BHoM"))
                    {
                        bhomAssemblies.Add(a);
                        if (a.FullName.Contains("_Engine"))
                        {
                            bhomEngineAssemblies.Add(a);

                            var methods = a.GetTypes()
                                .SelectMany(
                                    t => t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                    .Where(m => m.ReflectedType.Name == "Query"))
                                .Where(m => m.Name == "Geometry").ToList();

                            if (methods.Count != 0)
                                methods.ForEach(m => getGeometryMethods.Add(m.GetParameters().First().ParameterType, m));
                        }
                    }
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError("There was an issue retrieving the BHoM `Geometry()` methods.");
            }

            return getGeometryMethods;
        }
    }
}
