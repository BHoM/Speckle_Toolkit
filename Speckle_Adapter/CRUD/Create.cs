using BH.Adapter.Speckle.Types;
using BH.oM.Base;
using BH.oM.Data;
using BH.oM.Data.Collections;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/

        protected bool CreateIObjects(IEnumerable<IObject> objects)
        {
            IEnumerable<object> newObjects = (IEnumerable<object>)objects; //hacky; assumes T is always a reference type. Should be no problem anyway

            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(newObjects);
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            return true;
        }

        // Note: setAssignedId is currently not exposed as an option
        //  -- waiting for Adapter refactoring LVL 04 to expose a new CRUDconfig input for the Push 
        // CRUDconfig will become available to all CRUD methods
        protected bool CreateIBHoMObjects(IEnumerable<IBHoMObject> objects, bool setAssignedId = true)
        {
            /// Create the objects
            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(objects);
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);


            /// Assign any other property to the speckle objects before sending them
            var objList = objects.ToList();
            int i = 0;
            foreach (var o in SpeckleStream.Objects)
            {
                SpeckleStream.Objects[i].Name = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name;
                //SpeckleStream.Objects[i].Type = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name;
                i++;
            }

            /// Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });


            /// Read the IBHoMobjects as exported in speckle
            /// so we can assign the Speckle-generated id into the BHoMobjects
            if (setAssignedId)
            {

                ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

                IEnumerable<IBHoMObject> objectsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response, true).OfType<IBHoMObject>();

                VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(objects, objectsInSpeckle, new IBHoMGUIDComparer());

                if (correspondenceDiagram.Intersection.Count != objects.Count())
                {
                    Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                    return false;
                }

                correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterIdName] = o.Item2.CustomData[AdapterIdName]);

            }

            return true;
        }

        // Note: setAssignedId is currently not exposed as an option
        //  -- waiting for Adapter refactoring LVL 04 to expose a new CRUDconfig input for the Push 
        // CRUDconfig will become available to all CRUD methods
        protected bool CreateAnyObject(List<object> objects, bool setAssignedId = true)
        {
            /// Create the objects
            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(objects);
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            /// Assign any other property to the objects before sending them
            var objList = objects.ToList();
            int i = 0;
            foreach (var o in SpeckleStream.Objects)
            {
                IBHoMObject bhomObj = objList[i] as IBHoMObject;
                if (bhomObj != null)
                {
                    SpeckleStream.Objects[i].Name = string.IsNullOrEmpty(bhomObj.Name) ? bhomObj.GetType().ToString() : bhomObj.Name;
                    //SpeckleStream.Objects[i].Type = string.IsNullOrEmpty(bhomObj.Name) ? bhomObj.GetType().ToString() : bhomObj.Name;
                }
                i++;
            }

            /// Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });



            /// Read the IBHoMobjects as exported in speckle
            /// so we can assign the Speckle-generated id into the BHoMobjects
            if (setAssignedId)
            {
                ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

                List<IBHoMObject> bHoMObjects_inSpeckle = new List<IBHoMObject>();
                IEnumerable<IBHoMObject> iBhomObjsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response, true).OfType<IBHoMObject>();

                VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(objects.Where(o => o as IBHoMObject != null).Cast<IBHoMObject>(), iBhomObjsInSpeckle, new IBHoMGUIDComparer());

                if (correspondenceDiagram.Intersection.Count != objects.Count())
                {
                    var gna = 0;
                    //Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                    //return false;
                }

                correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterIdName] = o.Item2.CustomData[AdapterIdName]);

            }

            return true;
        }


        /***************************************************/
    }
}
