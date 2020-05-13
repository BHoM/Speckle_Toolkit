﻿/*
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

namespace BH.Adapter.Speckle
{
    public static partial class Convert
    {
        public static List<object> FromSpeckle(List<SpeckleObject> speckleObjects, bool storeSpeckleId = true)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            FromSpeckle(speckleObjects, out bhomObjects, out iObjects, out reminder, storeSpeckleId);

            return bhomObjects.Concat(iObjects).Concat(reminder).ToList();
        }

        public static bool FromSpeckle(List<SpeckleObject> speckleObjects,
                                    out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> nonBHoM,
                                    bool storeSpeckleId = true)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            nonBHoM = new List<object>();

            // Iterate all retrieved speckle objects
            for (int i = 0; i < speckleObjects.Count; i++)
            {
                object BHoMData = null;
                object deserialisedBHoMData = null;

                // If present, BHoM data is stored in the SpeckleObject's `Property` Dictionary in the `BHoM` key.
                if (speckleObjects[i].Properties.Count() != 0)
                {
                    // Extract the BHoMData.
                    speckleObjects[i].Properties.TryGetValue("BHoM", out BHoMData);

                    if (BHoMData != null)
                    {
                        if (BHoMData is string)
                        {
                            // The bhom data has been serialised using BHoM serialiser and is stored in a zipped json string.
                            BHoMData = BH.Engine.Serialiser.Convert.FromZip(BHoMData as string);
                            deserialisedBHoMData = BH.Engine.Serialiser.Convert.FromJson(BHoMData as string);
                        }
                        else if (BHoMData is SpeckleObject) // The bhom data has been serialised using speckle and is stored in a "SpeckleAbstract" object.
                            deserialisedBHoMData = SpeckleCore.Converter.Deserialise((SpeckleObject)BHoMData);

                        // Check if the deserialised data is a BHoMObject.
                        IBHoMObject iBHoMObject = deserialisedBHoMData as IBHoMObject;
                        if (iBHoMObject != null)
                        {
                            if (storeSpeckleId) // store the speckleId in the BHoMObject customData.
                                iBHoMObject.CustomData[AdapterIdName] = speckleObjects[i]._id;

                            bHoMObjects.Add(iBHoMObject);
                            continue;
                        }

                        // Check if the deserialised data is an IObject (which includes IGeometry).
                        IObject iObject = deserialisedBHoMData as IObject;
                        if (iObject != null)
                        {
                            iObjects.Add(iObject);
                            continue;
                        }
                    }
                }

                // If we got to here, the speckleObject is not BHoM.
                deserialisedBHoMData = speckleObjects[i];
                if (deserialisedBHoMData as SpeckleObject != null)
                    deserialisedBHoMData = SpeckleCore.Converter.Deserialise((SpeckleObject)deserialisedBHoMData);

                if (deserialisedBHoMData == null)
                    continue;

                //// Check if it's a Rhino Geometry.
                //Rhino.Geometry.GeometryBase rhinoGeom = deserialisedBHoMData as Rhino.Geometry.GeometryBase;
                //if (rhinoGeom != null)
                //    try
                //    {
                //        // Nurbsurface Pull currently doesn't work
                //        //var asd = rhinoGeom as Rhino.Geometry.NurbsSurface;

                //        // Try to convert that to a BHoM Geometry. 
                //        // This is because, just before Push, BHoM forces the convert of Rhino geometry to BHoMGeometry if any is found.
                //        deserialisedBHoMData = Rhinoceros.Convert.FromRhino(rhinoGeom);

                //        // If the convert succeded, this is now an IObject (IGeometry).
                //        iObjects.Add(deserialisedBHoMData as IObject);
                //        continue;
                //    }
                //    catch (Exception e)
                //    {
                //        BH.Engine.Reflection.Compute.RecordError($"BHoM could not convert some Rhino Geometry to BHoM Geometry: {e}");
                //    }

                // If all else failed, add this to the nonBHoM objects.
                nonBHoM.Add(deserialisedBHoMData);
            }

            return true;
        }

        // FOR TESTING ONLY
        private static bool CheckZippedData(SpeckleObject speckleObject)
        {
            // -------------------------- //  
            // For testing only           // 
            // -------------------------- // 

            // Extract BHoMData from the `BHoMZipped` key.
            // This data is serialised with our own Mongo Serialiser, and zipped to avoid modification from Speckle.

            object BHoMZippedData = null;

            try
            {
                object jsonBHoMDataObj = null;
                speckleObject.Properties.TryGetValue("BHoMZipped", out jsonBHoMDataObj);

                if (jsonBHoMDataObj != null)
                {
                    var jsonBHoMData = BH.Engine.Serialiser.Convert.FromZip(jsonBHoMDataObj.ToString()); //unzip
                    BHoMZippedData = BH.Engine.Serialiser.Convert.FromJson(jsonBHoMData); //deserialise
                }
            }
            catch { }

            return BHoMZippedData != null ? true : false;
        }
    }
}

