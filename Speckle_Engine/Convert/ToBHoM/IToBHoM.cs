/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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
