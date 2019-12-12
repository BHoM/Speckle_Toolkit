using BH.Adapter.Speckle.Types;
using BH.oM.Base;
using BH.oM.Data;
using BH.oM.Data.Collections;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects)
        {
            // This method gets always called by the base.Push --> objects are always IBHoMObjects

            //IEnumerable<IBHoMObject> bHoMObjects = objects.Cast<IBHoMObject>();
            return CreateIBHoMObjects(objects as dynamic);
        }

        protected bool CreateObjects(IEnumerable<object> objects)
        {
            // Convert the objects into "Abstract" SpeckleObjects 
            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(objects);

            // Add objects to the stream
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            // Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            return true;
        }

        // Note: setAssignedId is currently not exposed as an option
        //  -- waiting for Adapter refactoring LVL 04 to expose a new CRUDconfig input for the Push 
        // CRUDconfig will become available to all CRUD methods
        protected bool CreateIBHoMObjects(IEnumerable<IBHoMObject> BHoMObjects, bool setAssignedId = true)
        {
            // Convert the objects into the appropriate SpeckleObject (Point, Line, etc.) using the available converters.
            List<SpeckleObject> speckleObjects = BHoMObjects.Select(bhomObj => BH.Engine.Speckle.Convert.FromBHoM(bhomObj, m_GetGeometryMethods)).ToList();

            // Add objects to the stream
            SpeckleLayer.ObjectCount += BHoMObjects.Count();
            SpeckleStream.Objects.AddRange(speckleObjects);

            /// Assign any other property to the speckle objects before updating the stream
            var objList = BHoMObjects.ToList();
            int i = 0;
            foreach (var o in SpeckleStream.Objects)
            {
                SpeckleStream.Objects[i].Name = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name;
                //SpeckleStream.Objects[i].Type = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name;
                i++;
            }

            // Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });


            /// Read the IBHoMobjects as exported in speckle
            /// so we can assign the Speckle-generated id into the BHoMobjects
            if (setAssignedId)
            {

                ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

                IEnumerable<IBHoMObject> objectsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response, true);

                //VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(BHoMObjects, objectsInSpeckle, new IBHoMGUIDComparer());

                //if (correspondenceDiagram.Intersection.Count != BHoMObjects.Count())
                //{
                //    Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                //    return false;
                //}

                //correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterId] = o.Item2.CustomData[AdapterId]);

            }

            return true;
        }

        // Note: setAssignedId is currently not exposed as an option
        //  -- waiting for Adapter refactoring LVL 04 to expose a new CRUDconfig input for the Push 
        // CRUDconfig will become available to all CRUD methods
        protected bool CreateIObjects(List<IObject> objects, bool setAssignedId = true)
        {
            /// Convert the objects into SpeckleObjects and add them to the stream
            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(objects);
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            /// Update the stream
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            /// Read the IBHoMobjects as exported in speckle
            /// so we can assign the Speckle-generated id into the BHoMobjects
            if (setAssignedId)
            {
                ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

                List<IBHoMObject> bHoMObjects_inSpeckle = new List<IBHoMObject>();
                IEnumerable<IBHoMObject> iBhomObjsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response, true);

                VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(objects.Where(o => o as IBHoMObject != null).Cast<IBHoMObject>(), iBhomObjsInSpeckle, new IBHoMGUIDComparer());

                if (correspondenceDiagram.Intersection.Count != objects.Count())
                {
                    var gna = 0;
                    //Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                    //return false;
                }

                correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterId] = o.Item2.CustomData[AdapterId]);

            }

            return true;
        }


        /***************************************************/
    }
}
