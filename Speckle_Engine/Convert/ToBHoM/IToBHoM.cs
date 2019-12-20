using BH.oM.Base;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using SCG = SpeckleCoreGeometryClasses;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        public static IEnumerable<IBHoMObject> ToBHoM(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            ToBHoM(response, out bhomObjects, out iObjects, out reminder, setAssignedId, speckleIds);

            return bhomObjects;
        }

        public static bool ToBHoM(ResponseObject response,
                                    out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> nonBHoM,
                                    bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            nonBHoM = new List<object>();

            for (int i = 0; i < response.Resources.Count; i++)
            {
                // If we are filtering by speckleId, skip the object if its SpeckleId doesn't match.
                if (speckleIds != null && speckleIds.Count > 0)
                    if (!speckleIds.Any(id => id == response.Resources[i]._id)) // note: slow, o(n²)
                        continue;

                object resource = SpeckleCore.Converter.Deserialise(response.Resources[i]);
                object resource2 = SpeckleCore.Converter.Deserialise((SpeckleObject)response.Resources[i].Properties.First().Value);

                // BHoMData is always saved into the resource `Properties` Dictionary.
                if (response.Resources[i].Properties == null)
                {
                    // This is not a BHoM-related object.
                    nonBHoM.Add(resource);
                    continue;
                }

                // Otherwise, there might be BHoMData in this resource.
                object BHoMData = null;

                if (response.Resources[i].Properties.ContainsKey("BHoMData"))
                {
                    // There is BHoMData.
                    // BHoMData which is always a JSON representation of either an IObject, an IBHoMObject or an IGeometry.
                    string jsonBHoMData = response.Resources[i].Properties["BHoMData"].ToString();

                    try
                    {
                        jsonBHoMData = BH.Engine.Serialiser.Convert.FromZip(jsonBHoMData); //unzip
                        BHoMData = BH.Engine.Serialiser.Convert.FromJson(jsonBHoMData); //deserialise
                    }
                    catch { }

                    if (BHoMData == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("There was a problem deserialising BHoM data from a pulled Speckle resource");
                        continue;
                    }
                }

                // Check if it's a BHoMObject.
                IBHoMObject iBHoMObject = BHoMData as IBHoMObject;
                if (iBHoMObject != null)
                {
                    if (assignSpeckleIdToBHoMObjects)
                        iBHoMObject.CustomData[AdapterId] = response.Resources[i]._id;

                    bHoMObjects.Add(iBHoMObject);
                    continue;
                }

                // Check if it's a IObject (which includes IGeometry).
                IObject iObject = BHoMData as IObject;
                if (iObject != null)
                {
                    iObjects.Add(iObject);
                    continue;
                }

                // If got to here, it's neither.
                nonBHoM.Add(resource);
            }

            return true;
        }
    }
}
