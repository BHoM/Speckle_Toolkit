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
using Rhino;
using BH.Engine.Rhinoceros;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Wraps the content of a BHoMObject into a SpeckleObject.\n" +
            "Its geometry (if any) is exposed in the top level, so it can be visualised in Speckle.\n" +
            "All BHoMObject properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IBHoMObject bhomObject)
        {
            // This will be our return object.
            // On the higher level, it will contain geometry to be represented in SpeckleViewer.
            // Nested into its `.Properties` it will also "wrap" any other BHoM data.
            SpeckleObject speckleObject = null;

            // See if BHoMObject has a direct convertion to a SpeckleObject
            speckleObject = FromBHoM(bhomObject as dynamic, false);

            if (speckleObject == null)
            {
                // Else, see if we can get a BHoM geometry out of the BHoMObject to represent the object in SpeckleViewer.
                IGeometry geom = null;
                geom = bhomObject.IGeometry();

                if (geom != null)
                {
                    // Converts the BHoM Geometry into a SpeckleObject, dynamically dispatching to the method for the right type.
                    speckleObject = IFromBHoM(geom as dynamic); 
                }
                else
                {
                    // BHoMObject does not have a geometrical representation in BHoM.
                    // It must be converted to an "Abstract" SpeckleObject.
                    speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
                }
            }

            // Serialise the BHoMobject into a Json and append it to the Properties Dictionary of the SpeckleObject. Key is "BHoMData".
            string BHoMDataJson = BH.Engine.Serialiser.Convert.ToJson(bhomObject);
            speckleObject.Properties = new Dictionary<string, object>() { { "BHoMData", BHoMDataJson } };

            return speckleObject;
        }

        [Description("Wraps the content of a BHoM IGeometry into a SpeckleObject.\n" +
           "A Rhino geometry representation is extracted and exposed in the top level, so it can be visualised in Speckle.\n" +
           "All the other IGeometry properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IGeometry iGeometry)
        {
            var rhinoGeom = Engine.Rhinoceros.Convert.IToRhino(iGeometry);

            // Creates the SpeckleObject with the Rhino Geometry. 
            var speckleObj_rhinoGeom = (SpeckleObject)SpeckleCore.Converter.Serialise(rhinoGeom); // This will be our "wrapper" object for the rest of the IObject stuff.

            // Serialise the iGeometry into a Json and append it to the additional properties of the SpeckleObject.
            BH.Engine.Serialiser.Convert.ToJson(iGeometry);
            speckleObj_rhinoGeom.Properties = new Dictionary<string, object>() { { iGeometry.GetType().Name, BH.Engine.Serialiser.Convert.ToJson(iGeometry) } };

            return speckleObj_rhinoGeom;
        }
    }
}
