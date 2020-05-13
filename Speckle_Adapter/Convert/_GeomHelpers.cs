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
using BH.Engine.Rhinoceros;
using BH.oM.Speckle;
using BH.oM.Reflection.Attributes;

namespace BH.Adapter.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/
        // Helper Methods for SpeckleCoreGeometry

        private static double[] ToFlatArray(this IEnumerable<BHG.Point> points)
        {
            return points.SelectMany(pt => pt.ToArray()).ToArray();
        }

        private static double[] ToArray(this BHG.Point pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

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
