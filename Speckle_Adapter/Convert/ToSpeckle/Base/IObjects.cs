/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Data;
using BH.oM.Data.Collections;
using BH.oM.Geometry;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Speckle;
using BH.oM.Speckle;


namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        protected SpeckleObject ToSpeckle(IObject iObject, SpecklePushConfig config)
        {
            // Convert the objects into the appropriate SpeckleObject using the available converters.
            SpeckleObject speckleObject = null;

            if (typeof(IGeometry).IsAssignableFrom(iObject.GetType()))
                speckleObject = Speckle.Convert.IToSpeckle((IGeometry)iObject);

            if (speckleObject == null)
            {
                BH.oM.Graphics.RenderMesh rm = null;

                try
                {
                    rm = BH.Engine.Representation.Compute.IRenderMesh(iObject);
                }
                catch
                {
                    BH.Engine.Reflection.Compute.RecordNote($"Could not compute the representation for an object of type {iObject.GetType().Name}.\n" +
                        $"This simply means that the object will not be viewable in the browser (SpeckleViewer).");
                }

                if (rm != null)
                    speckleObject = Speckle.Convert.ToSpeckle(rm);
                else
                    speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(iObject); // These will be exported as `Abstract` Speckle Objects.
            }

            // Save BHoMObject data inside the speckleObject.
            Modify.SetBHoMData(speckleObject, iObject, config.UseSpeckleSerialiser);

            speckleObject.SetDiffingHash(iObject, config);

            return speckleObject;
        }
    }
}




