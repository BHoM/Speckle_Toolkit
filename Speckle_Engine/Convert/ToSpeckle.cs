using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;

namespace BH.Engine.Speckle
{
  public static partial class Convert
  {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        //Add methods for converting From BHoM to the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static SpeckleNode ToSpeckle(this Node node)
        //{
        //    //Insert code for convertion
        //}

        /***************************************************/

        /// <summary>
        /// Extension method to convert bhom meshes to speckle meshes. 
        /// Will get called automatically in the speckle "Serialise" method.
        /// https://github.com/speckleworks/SpeckleCore/blob/9545e96f04d85f46203a99c21c76eeea0ea03dae/SpeckleCore/Conversion/ConverterSerialisation.cs#L94
        /// </summary>

        public static object ToSpeckle(this BH.oM.Geometry.Mesh bhomMesh)
        {

            SpeckleCoreGeometryClasses.SpeckleMesh speckleMesh = new SpeckleCoreGeometryClasses.SpeckleMesh();
            List<BH.oM.Geometry.Face> bhomFaces = bhomMesh.Faces;
            List<BH.oM.Geometry.Point> bhomPts = bhomMesh.Vertices;

            List<double> speckleVertices = new List<double>();
            foreach (var bhomPt in bhomPts)
            {
                speckleVertices.Add(bhomPt.X);
                speckleVertices.Add(bhomPt.Y);
                speckleVertices.Add(bhomPt.Z);
            }

            List<int> speckleFaces = new List<int>();
            foreach (var bhomFace in bhomFaces)
            {
                if(bhomFace.D == -1)
                {
                    speckleFaces.Add(0);
                    speckleFaces.Add(bhomFace.A);
                    speckleFaces.Add(bhomFace.B);
                    speckleFaces.Add(bhomFace.C);
                }
                else
                {
                    speckleFaces.Add(1);
                    speckleFaces.Add(bhomFace.A);
                    speckleFaces.Add(bhomFace.B);
                    speckleFaces.Add(bhomFace.C);
                    speckleFaces.Add(bhomFace.D);
                }
            }

            speckleMesh.Vertices = speckleVertices;
            speckleMesh.Faces = speckleFaces;
            //speckleMesh.Hash TODO: generate hash

            return speckleMesh;
        }


        public static void test()
        {
            BH.oM.Geometry.Mesh bhomMesh = new oM.Geometry.Mesh();

            object speckleMesh = bhomMesh.ToSpeckle();

        }

    }
}
