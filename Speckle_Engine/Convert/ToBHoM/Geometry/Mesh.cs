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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BHG = BH.oM.Geometry;
using System.ComponentModel;
using SCG = SpeckleCoreGeometryClasses;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        // -------------------------------------------------------------------------------- //
        // NOTE
        // These ToBHoM methods are not automatically called by any method in the Toolkit,
        // as the deserialisation already brings back the BHoM object.
        // Kept for reference and for manual use in the UI.
        // -------------------------------------------------------------------------------- //


        [Description("Convert Speckle Mesh to BHoM Mesh")]
        public static BHG.Mesh ToBHoM(this SCG.SpeckleMesh speckleMesh)
        {
            List<BHG.Point> vertices = speckleMesh.Vertices.ToPoints();
            List<int> sfaces = speckleMesh.Faces;
            List<BHG.Face> faces = new List<BHG.Face>();
            for (int i = 0; i < sfaces.Count;)
            {
                if (sfaces[i] == 1) // triangle face
                {
                    faces.Add(new BHG.Face { A = sfaces[i + 1], B = sfaces[i + 2], C = sfaces[i + 3], D = -1 });
                    i = i + 4;
                }
                if (sfaces[i] == 0) // quad face
                {
                    faces.Add(new BHG.Face { A = sfaces[i + 1], B = sfaces[i + 2], C = sfaces[i + 3], D = sfaces[i + 4] });
                    i = i + 5;
                }
            }

            return new BHG.Mesh { Vertices = vertices, Faces = faces };
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
