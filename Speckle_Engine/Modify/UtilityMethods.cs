using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static class Query
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

            DispatchBHoMObjects(objects, out ibhomObjects, out iObjects, out List<object> reminder);
        }

        public static void DispatchBHoMObjects(IEnumerable<object> objects, out List<IBHoMObject> ibhomObjects, out List<IObject> iObjects, out List<object> reminder)
        {
            ibhomObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            reminder = new List<object>();

            Type iBHoMObjectType = typeof(IBHoMObject);
            Type iObjectType = typeof(IObject);

            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");

            foreach (var typeGroup in objects.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var castedObjects = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They're iBHoMObjects
                    ibhomObjects.AddRange((IEnumerable<IBHoMObject>)castedObjects);
                }
                else if (iObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They're iObjects
                    iObjects.AddRange((IEnumerable<IObject>)castedObjects);
                } else
                {
                    // They're something else
                    reminder.AddRange((IEnumerable<object>)castedObjects);
                }
            }
        }

        public static IEnumerable<IBHoMObject> FilterByBHoMGUID(IEnumerable<IBHoMObject> objects, List<string> bhomGuid)
        {
            return objects.Where(o => bhomGuid.Any(id => id == o.BHoM_Guid.ToString())); //Inefficient o(n*m) -- implement some kind of sorting https://stackoverflow.com/a/25184658/3873799
        }
    }
}
