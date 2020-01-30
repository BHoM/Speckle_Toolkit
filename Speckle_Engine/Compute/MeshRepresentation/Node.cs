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
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.Engine.Rhinoceros;
using BH.oM.Structure.Constraints;

namespace BH.Engine.Speckle
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a mesh representation for the Node based on its DOF, e.g. a box for fully fixed, a cone with sphere on top for pin.")]
        public static Rhino.Geometry.Mesh MeshRepresentation(this Node node)
        {
            if (node.Position == null)
            {
                Reflection.Compute.RecordError("Specified Node does not have a position defined.");
                return null;
            }

            var point = (Rhino.Geometry.Point3d)node.Position.IToRhino();
            double scale = 2;

            // Mesh to represent the node constraint
            Rhino.Geometry.Mesh mesh = null;

            if (node.Support == null)
                return new Rhino.Geometry.Mesh();

            // Different 3d representation for different DOF
            var fixedDOFTypes = new[] { oM.Structure.Constraints.DOFType.Fixed, oM.Structure.Constraints.DOFType.FixedNegative, oM.Structure.Constraints.DOFType.FixedPositive,
            oM.Structure.Constraints.DOFType.Spring, oM.Structure.Constraints.DOFType.Friction, oM.Structure.Constraints.DOFType.Damped, oM.Structure.Constraints.DOFType.SpringPositive, oM.Structure.Constraints.DOFType.SpringNegative};
            bool fixedToTranslation = fixedDOFTypes.Contains(node.Support.TranslationX) || fixedDOFTypes.Contains(node.Support.TranslationY) || fixedDOFTypes.Contains(node.Support.TranslationZ);
            bool fixedToRotation = fixedDOFTypes.Contains(node.Support.RotationX) || fixedDOFTypes.Contains(node.Support.RotationY) || fixedDOFTypes.Contains(node.Support.RotationZ);

            if (fixedToTranslation && fixedToRotation)
            {
                // Fully fixed: box
                double boxDims = 0.06 * scale;
                var rhinoBox = new Rhino.Geometry.BoundingBox(new[] { point, new Rhino.Geometry.Point3d(point.X + 2 * boxDims, point.Y + 2 * boxDims, point.Z), new Rhino.Geometry.Point3d(point.X - 2 * boxDims, point.Y - 2 * boxDims, point.Z - 3 * boxDims) });
                mesh = Rhino.Geometry.Mesh.CreateFromBox(rhinoBox, 1, 1, 1);
            }
            else if (fixedToTranslation && !fixedToRotation)
            {
                // Pin: cone + sphere
                double radius = 0.06 * scale;

                var rhinoSphere = new Rhino.Geometry.Sphere(new Rhino.Geometry.Plane(point, new Rhino.Geometry.Vector3d(0, 0, 1)), radius);
                mesh = Rhino.Geometry.Mesh.CreateFromSphere(rhinoSphere, 8, 4);

                var xyPlane = new Rhino.Geometry.Plane(new Rhino.Geometry.Point3d(point.X, point.Y, point.Z - radius), new Rhino.Geometry.Vector3d(0, 0, -1));
                Rhino.Geometry.Cone cone = new Rhino.Geometry.Cone(xyPlane, 5 * radius, 3 * radius);
                var coneMesh = Rhino.Geometry.Mesh.CreateFromCone(cone, 1, 8);

                mesh.Append(coneMesh);
            }

            //SpeckleCircle speckleCircle = new SpeckleCircle(new SpecklePlane(new SpecklePoint(point.X, point.Y, point.Z), new SpeckleVector(0, 0, 1), new SpeckleVector(1, 0, 0), new SpeckleVector(0, 1, 0)), 0.5);
            //SpeckleCircle speckleCircle2 = new SpeckleCircle(new SpecklePlane(new SpecklePoint(point.X, point.Y, point.Z), new SpeckleVector(0, 1, 0), new SpeckleVector(1, 0, 0), new SpeckleVector(0, 1, 0)), 0.5);

            ////SpeckleVector speckleVector = new SpeckleVector()
            //SpeckleObject asd = new SpeckleObject();
            //asd.Properties.Add("asd", speckleCircle);
            //asd.Properties.Add("asd2", speckleCircle2);

            if (mesh == null && false) // optional, deactivated for now
            {
                // Just make a little sphere
                double radius = 0.06 * scale;
                var rhinoSphere = new Rhino.Geometry.Sphere(new Rhino.Geometry.Plane(point, new Rhino.Geometry.Vector3d(0, 0, 1)), radius);
                mesh = Rhino.Geometry.Mesh.CreateFromSphere(rhinoSphere, 8, 4);
            }

            return mesh;
        }

    }
}

