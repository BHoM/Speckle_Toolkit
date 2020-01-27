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

namespace BH.Engine.Speckle
{
  public static partial class Convert
  {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<IBHoMObject> ToBHoM_onlyBHoMObjects(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            ToBHoM(response, out bhomObjects, out iObjects, out reminder, setAssignedId, speckleIds);

            return bhomObjects;
        }

        public static IEnumerable<object> ToBHoM(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            ToBHoM(response, out bhomObjects, out iObjects, out reminder, setAssignedId, speckleIds);

            return bhomObjects.Cast<object>().Concat(iObjects.Cast<object>()).Concat(reminder);
        }


        public static bool ToBHoM(ResponseObject response, out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder, bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
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
                        iBHoMObject.CustomData[AdapterIdName] = response.Resources[i]._id;

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


        /// <summary>
        /// Extension method to convert bhom meshes to speckle meshes. 
        /// Will get called automatically in the speckle "Deserialise" method.
        /// https://github.com/speckleworks/SpeckleCore/blob/9545e96f04d85f46203a99c21c76eeea0ea03dae/SpeckleCore/Conversion/ConverterDeserialisation.cs#L64
        /// </summary>
        //public static BH.oM.Geometry.Mesh ToNative (this Specklemesh speckleMesh)
        //{
        //    BH.oM.Geometry.Mesh bhomMesh = new BH.oM.Geometry.Mesh();

        //    // Conversion stuff

        //    return bhomMesh;
        //}
    }
}
