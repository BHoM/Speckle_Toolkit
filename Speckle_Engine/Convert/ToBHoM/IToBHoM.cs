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
        public static IEnumerable<IBHoMObject> ToBHoM(List<SpeckleObject> speckleObjects, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            ToBHoM(speckleObjects, out bhomObjects, out iObjects, out reminder, setAssignedId, speckleIds);

            return bhomObjects;
        }

        public static bool ToBHoM(List<SpeckleObject> speckleObjects,
                                    out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> nonBHoM,
                                    bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            nonBHoM = new List<object>();

            for (int i = 0; i < speckleObjects.Count; i++)
            {
                // If we are filtering by speckleId, skip the object if its SpeckleId doesn't match.
                if (speckleIds != null && speckleIds.Count > 0)
                    if (!speckleIds.Any(id => id == speckleObjects[i]._id)) // note: slow, o(n²)
                        continue;

                object BHoMData = null;
                object deserialisedBHoMData = null;

                if (speckleObjects[i].Properties.Count() != 0)
                {
                    speckleObjects[i].Properties.TryGetValue("BHoM", out BHoMData);


                    if (BHoMData is SpeckleObject)
                        deserialisedBHoMData = SpeckleCore.Converter.Deserialise((SpeckleObject)BHoMData);


                    // Check if Speckle deserialisation was indeed successful.
                    if (deserialisedBHoMData as IBHoMObject == null && deserialisedBHoMData as IObject == null)
                    {
                        try
                        {
                            object jsonBHoMDataObj = null;
                            speckleObjects[i].Properties.TryGetValue("BHoMData", out jsonBHoMDataObj);

                            if (jsonBHoMDataObj != null)
                            {
                                var jsonBHoMData = BH.Engine.Serialiser.Convert.FromZip(jsonBHoMDataObj.ToString()); //unzip
                                deserialisedBHoMData = BH.Engine.Serialiser.Convert.FromJson(jsonBHoMData); //deserialise
                            }
                        }
                        catch { }
                    }


                    // Check if it's a BHoMObject.
                    IBHoMObject iBHoMObject = deserialisedBHoMData as IBHoMObject;
                    if (iBHoMObject != null)
                    {
                        if (assignSpeckleIdToBHoMObjects)
                            iBHoMObject.CustomData[AdapterId] = speckleObjects[i]._id;

                        bHoMObjects.Add(iBHoMObject);
                        continue;
                    }

                    // Check if it's a IObject (which includes IGeometry).
                    IObject iObject = deserialisedBHoMData as IObject;
                    if (iObject != null)
                    {
                        iObjects.Add(iObject);
                        continue;
                    }
                }

                deserialisedBHoMData = speckleObjects[i];
                if (deserialisedBHoMData as SpeckleObject != null)
                    deserialisedBHoMData = SpeckleCore.Converter.Deserialise((SpeckleObject)deserialisedBHoMData);
                    if (deserialisedBHoMData != null)
                        nonBHoM.Add(deserialisedBHoMData);


               
            }

            return true;
        }
    }
}
