using BH.oM.Base;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCoreGeometryClasses;
using Rhino;

namespace BH.Engine.Speckle
{
  public static partial class Convert
  {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<IBHoMObject> ToBHoM(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();

            ToBHoM(response, out bhomObjects, out _, out _, setAssignedId, speckleIds);

            return bhomObjects;
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
                        iBHoMObject.CustomData[AdapterId] = response.Resources[i]._id;

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
        public static BH.oM.Geometry.Mesh ToNative(this SpeckleCoreGeometryClasses.SpeckleMesh speckleMesh)
        {

            BH.oM.Geometry.Mesh bhomMesh = new BH.oM.Geometry.Mesh();

            List<BH.oM.Geometry.Point> bhomVertices = new List<BH.oM.Geometry.Point>();
            for (int j = 0; j < speckleMesh.Vertices.Count; j++)
            {

                if((j%3) == 0)
                {
                    BH.oM.Geometry.Point bhomPoint = new BH.oM.Geometry.Point();
                    bhomPoint.X = speckleMesh.Vertices[j];
                    bhomPoint.Y = speckleMesh.Vertices[j +1];
                    bhomPoint.Z = speckleMesh.Vertices[j + 2];
                    bhomVertices.Add(bhomPoint);
                }
            }
            
            List<BH.oM.Geometry.Face> bhomFaces = new List<BH.oM.Geometry.Face>();
            int i = 0;
            while (i < speckleMesh.Faces.Count)
            {
                if (speckleMesh.Faces[i] == 0)
                { // triangle
                    BH.oM.Geometry.Face bhomFace = new BH.oM.Geometry.Face();
                    bhomFace.A = speckleMesh.Faces[i + 1];
                    bhomFace.B = speckleMesh.Faces[i + 2];
                    bhomFace.C = speckleMesh.Faces[i + 3];
                    bhomFace.D = -1;
                    bhomFaces.Add(bhomFace);
                    i += 4;
                }
                else
                { // quad
                    BH.oM.Geometry.Face bhomFace = new BH.oM.Geometry.Face();
                    bhomFace.A = speckleMesh.Faces[i + 1];
                    bhomFace.B = speckleMesh.Faces[i + 2];
                    bhomFace.C = speckleMesh.Faces[i + 3];
                    bhomFace.D = speckleMesh.Faces[i + 4];
                    bhomFaces.Add(bhomFace);
                    i += 5;
                }
            }

            bhomMesh.Vertices = bhomVertices;
            bhomMesh.Faces = bhomFaces;

            return bhomMesh;
        }
    }
}
