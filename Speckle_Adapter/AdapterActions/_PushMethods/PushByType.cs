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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using BH.oM.Speckle;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        private bool PushByType(List<object> objectsToPush, string tag, SpecklePushConfig config)
        {
            bool success = true;

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
                        // They are IBHoMObjects.

                        // Assign SpeckleStreamId to the CustomData of the IBHoMObjects
                        var iBHoMObjects = (list as IEnumerable<IBHoMObject>).ToList();
                        iBHoMObjects.ForEach(o => o.CustomData["Speckle_StreamId"] = SpeckleStreamId);

                        success &= CreateIBHoMObjects(iBHoMObjects as dynamic, config);

                        if (config.StoreSpeckleId)
                            StoreSpeckleId(iBHoMObjects);
                    }
                    else
                    {
                        // They are simply IObjects.
                        var iObjects = (list as IEnumerable<IObject>).ToList();
                        success &= CreateIObjects(iObjects as dynamic, config);
                    }
                }
                else
                {
                    // They are something else.
                    // These objects will be exported as "Abstract" SpeckleObjects.
                    success &= CreateObjects(list as dynamic, config);
                }
            }

            return success;
        }
    }
}