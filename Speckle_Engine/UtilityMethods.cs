using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static class UtilityMethods
    {
        public static IEnumerable<IBHoMObject> FilterIBHoMObjects(IEnumerable<object> objects)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();

            Type iBHoMObjectType = typeof(IBHoMObject);

            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");

            foreach (var typeGroup in objects.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var castedObjects = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    bhomObjects.AddRange((IEnumerable<IBHoMObject>)castedObjects);
                }
            }

            return bhomObjects;
        }

        public static void DispatchBHoMObjects(IEnumerable<IObject> objects, out List<IBHoMObject> ibhomObjects, out List<IObject> iObjects)
        {
            ibhomObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();

            Type iBHoMObjectType = typeof(IBHoMObject);

            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");

            foreach (var typeGroup in objects.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var castedObjects = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    ibhomObjects.AddRange((IEnumerable<IBHoMObject>)castedObjects);
                }
                else
                {
                    // They're iObjects
                    iObjects.AddRange((IEnumerable<IObject>)castedObjects);
                }
            }

            
        }

        public static IEnumerable<IBHoMObject> FilterByBHoMGUID(IEnumerable<IBHoMObject> objects, List<string> bhomGuid)
        {
            return objects.Where(o => bhomGuid.Any(id => id == o.BHoM_Guid.ToString())); //Inefficient o(n*m) -- implement some kind of sorting https://stackoverflow.com/a/25184658/3873799
        }
    }
}
