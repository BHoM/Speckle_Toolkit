using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        private void BasePushRewritten(List<IObject> objectsToPush, string pushType, string tag, bool setAssignedId, ref bool success)
        {
            // // - Base push rewritten to allow some additional CustomData to go in.
            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");
            foreach (var typeGroup in objectsToPush.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });
                var list = miListObject.Invoke(typeGroup, new object[] { typeGroup });


                if ((typeof(IObject).IsAssignableFrom(typeGroup.Key)))
                {
                    // They are IObjects = all types within BHoM (now not needed, soon will be for the Adapter refactoring).
                    // These guys might be either IBHoMObjects (=complex objects)
                    // or IGeometries/other types inheriting only from IObject.

                    if (typeof(IBHoMObject).IsAssignableFrom(typeGroup.Key))
                    {
                        // They are IBHoMObjects

                        // Assign SpeckleStreamId to the CustomData of the IBHoMObjects
                        var iBHoMObjects = list as IEnumerable<IBHoMObject>;
                        iBHoMObjects.ToList().ForEach(o => o.CustomData["Speckle_StreamId"] = SpeckleStreamId);

                        success &= CreateIBHoMObjects(iBHoMObjects as dynamic, setAssignedId);
                    }
                    else
                    {
                        // They are simply IObjects.
                        var iObjects = (list as IEnumerable<IObject>).ToList();
                        success &= CreateIObjects(iObjects as dynamic, setAssignedId);

                    }
                }
                else
                {
                    // They are something else.
                    // These objects will be exported as "Abstract" SpeckleObjects.
                    CreateObjects(list as dynamic);
                }
            }
        }
    }
}