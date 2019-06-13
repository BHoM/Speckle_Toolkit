using BH.oM.Base;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
  public static partial class Convert
  {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<IBHoMObject> ResponseToBHoM(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();

            ResponseToBHoM(response, out bhomObjects, out _, out _, setAssignedId, speckleIds);

            return bhomObjects;
        }

        public static bool ResponseToBHoM(ResponseObject response, out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder, bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            reminder = new List<object>();

            for (int i = 0; i < response.Resources.Count; i++)
            {
                var resource = Converter.Deserialise(response.Resources[i]);

                if (speckleIds != null && speckleIds.Count > 0)
                    if (!speckleIds.Any(id => id == response.Resources[i]._id))
                        continue;

                IBHoMObject iBHoMObject = resource as IBHoMObject;
                if (iBHoMObject != null)
                {
                    if (assignSpeckleIdToBHoMObjects)
                        iBHoMObject.CustomData[AdapterId] = response.Resources[i]._id;

                    bHoMObjects.Add(iBHoMObject);
                    continue;
                }
                var iObject = resource as IObject;

                if (iObject != null)
                {
                    iObjects.Add(iObject);
                    continue;
                }

                reminder.Add(resource);
            }

            return true;
        }


    }
}
