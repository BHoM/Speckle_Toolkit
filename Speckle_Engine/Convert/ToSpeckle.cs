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

        //public static object ToSpeckle(this BH.oM.Geometry.Mesh bhomMesh)
        //{
        //    object specklemesh = null;

        //    // Write conversion here


        //    return specklemesh;
        //}


        //public static void test()
        //{
        //    BH.oM.Geometry.Mesh bhomMesh = new oM.Geometry.Mesh();

        //    object speckleMesh = bhomMesh.ToSpeckle();

        //}

    }
}
