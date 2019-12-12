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
        public static IEnumerable<IBHoMObject> GetOnlyIBHoMObjects(IEnumerable<object> objects)
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
    }
}
