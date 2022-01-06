/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Speckle;
using System.Drawing;

namespace BH.Adapter.Speckle
{
    public static partial class Convert
    {
        [Description("Convert BHoM Mesh to a Speckle Mesh")]
        public static SpeckleMesh ToSpeckle(this BHG.Mesh bhomMesh, Color? colour = null)
        {
            double[] vertices = bhomMesh.Vertices.ToFlatArray();
            int[] faces = bhomMesh.Faces.SelectMany(face =>
            {
                if (face.D != -1) return new int[] { 1, face.A, face.B, face.C, face.D };
                return new int[] { 0, face.A, face.B, face.C };
            }).ToArray();

            Color col = System.Drawing.Color.FromArgb(255, 100, 100, 100);

            if (colour != null)
                col = (Color)colour;

            int[] colors = Enumerable.Repeat(col.ToArgb(), vertices.Count()).ToArray();

            SpeckleMesh speckleMesh = new SpeckleMesh(vertices, faces, colors, null);

            return speckleMesh;
        }
    }
}


