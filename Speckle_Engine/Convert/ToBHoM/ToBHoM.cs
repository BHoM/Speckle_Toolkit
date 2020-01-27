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
using BH.oM.Reflection.Attributes;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using SCG = SpeckleCoreGeometryClasses;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert Speckle Point to BHoM Point")]
        public static BHG.Point ToBHoM(this SCG.SpecklePoint specklePoint)
        {
            return new BHG.Point { X = specklePoint.Value[0], Y = specklePoint.Value[1], Z = specklePoint.Value[2] };
        }

        [Description("Convert Speckle Vector to BHoM Vector")]
        public static BHG.Vector ToBHoM(this SCG.SpeckleVector speckleVector)
        {
            return new BHG.Vector { X = speckleVector.Value[0], Y = speckleVector.Value[1], Z = speckleVector.Value[2] };
        }

        [Description("Convert Speckle Line to BHoM Line")]
        public static BHG.Line ToBHoM(this SCG.SpeckleLine speckleLine)
        {
            List<BHG.Point> points = speckleLine.Value.ToPoints();
            return new BHG.Line { Start = points[0], End = points[1] };
        }

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


        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        [Description("Mass point converter adapted from SpeckleCoreGeometry. It takes IEnumerable instead of array.")]
        [Input("arr", "Flat array of coordinates from Speckle")]
        [Output("List of BHoM Points")]
        private static List<BHG.Point> ToPoints(this IEnumerable<double> arr)
        {
            if (arr.Count() % 3 != 0)
                throw new Exception("Array malformed: length%3 != 0.");

            List<BHG.Point> points = new List<BHG.Point>();
            var asArray = arr.ToArray();
            for (int i = 2, k = 0; i < arr.Count(); i += 3)
                points[k++] = new BHG.Point { X = asArray[i - 2], Y = asArray[i - 1], Z = asArray[i] };

            return points;
        }
    }
}
