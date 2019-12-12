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

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert BHoM Point to a Speckle Point")]
        public static SpecklePoint FromBHoM(this BHG.Point bhomPoint)
        {
            if (bhomPoint == null) return default(SpecklePoint);

            SpecklePoint specklePoint = new SpecklePoint(bhomPoint.X, bhomPoint.Y, bhomPoint.Z);

            specklePoint.GenerateHash();
            return specklePoint;
        }

        [Description("Convert BHoM Vector to a Speckle Vector")]
        public static SpeckleVector FromBHoM(this BHG.Vector bhomVector)
        {
            if (bhomVector == null) return default(SpeckleVector);

            SpeckleVector speckleVector = new SpeckleVector(bhomVector.X, bhomVector.Y, bhomVector.X);

            speckleVector.GenerateHash();
            return speckleVector;
        }

        [Description("Convert BHoM Line to a Speckle Line")]
        public static SpeckleLine FromBHoM(this BHG.Line bhomLine)
        {
            if (bhomLine == null) return default(SpeckleLine);

            SpeckleLine speckleLine = new SpeckleLine(
                (new BHG.Point[] { bhomLine.Start, bhomLine.End }).ToFlatArray()
                );

            speckleLine.GenerateHash();
            return speckleLine;
        }

        [Description("Convert BHoM Mesh to a Speckle Mesh")]
        public static SpeckleMesh FromBHoM(this BHG.Mesh bhomMesh)
        {
            double[] vertices = bhomMesh.Vertices.ToFlatArray();
            int[] faces = bhomMesh.Faces.SelectMany(face =>
            {
                if (face.D != -1) return new int[] { 1, face.A, face.B, face.C, face.D };
                return new int[] { 0, face.A, face.B, face.C };
            }).ToArray();
            var defaultColour = System.Drawing.Color.FromArgb(255, 100, 100, 100);
            var colors = Enumerable.Repeat(defaultColour.ToArgb(), vertices.Count()).ToArray();

            SpeckleMesh speckleMesh = new SpeckleMesh(vertices, faces, colors, null);

            speckleMesh.GenerateHash();
            return speckleMesh;
        }


        /// <summary>
        /// Extension method to convert bhom meshes to speckle meshes. 
        /// Will get called automatically in the speckle "Serialise" method.
        /// https://github.com/speckleworks/SpeckleCore/blob/9545e96f04d85f46203a99c21c76eeea0ea03dae/SpeckleCore/Conversion/ConverterSerialisation.cs#L94
        /// </summary>
        //public static object FromBHoM(this BH.oM.Geometry.Mesh bhomMesh)
        //{
        //    object specklemesh = null;

        //    // Write conversion here


        //    return specklemesh;
        //}

        //public static void test()
        //{
        //    BH.oM.Geometry.Mesh bhomMesh = new oM.Geometry.Mesh();

        //    object speckleMesh = bhomMesh.FromBHoM();

        //}


        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/
        // Helper Methods for SpeckleCoreGeometry

        private static double[] ToArray(this BHG.Point pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

        private static double[] ToFlatArray(this IEnumerable<BHG.Point> points)
        {
            return points.SelectMany(pt => pt.ToArray()).ToArray();
        }
    }
}
