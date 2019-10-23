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

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            // This method gets always called by the base.Push --> objects are always IBHoMObjects

            //IEnumerable<IBHoMObject> bHoMObjects = objects.Cast<IBHoMObject>();
            return CreateIBHoMObjects(objects as dynamic, replaceAll);
        }

        protected bool CreateIObjects(IEnumerable<IObject> objects, bool replaceAll = false)
        {
            IEnumerable<object> newObjects = (IEnumerable<object>)objects; //hacky; assumes T is always a reference type. Should be no problem anyway

            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(newObjects);
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            return true;
        }

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

                IEnumerable<IBHoMObject> objectsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response, true);

                VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(objects, objectsInSpeckle, new IBHoMGUIDComparer());

                if (correspondenceDiagram.Intersection.Count != objects.Count())
                {
                    Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                    return false;
                }

                correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterId] = o.Item2.CustomData[AdapterId]);

            }

            return true;
        }

        protected bool CreateAnyObject(List<IObject> objects, bool setAssignedId = true)
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

        ///***************************************************/
        ///**** Private methods                           ****/
        ///***************************************************/

        //private bool CreateCollection( IEnumerable<Bar> bars )
        //{
        //  //Code for creating a collection of bars in the software


        //  foreach ( Bar bar in bars )
        //  {
        //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
        //    object barId = bar.CustomData[ AdapterId ];
        //    //If also the default implmentation for the DependencyTypes is used,
        //    //one can from here get the id's of the subobjects by calling (cast into applicable type used by the software): 
        //    object startNodeId = bar.StartNode.CustomData[ AdapterId ];
        //    object endNodeId = bar.EndNode.CustomData[ AdapterId ];
        //    object SecPropId = bar.SectionProperty.CustomData[ AdapterId ];
        //  }


        //  throw new NotImplementedException();
        //}

        ///***************************************************/

        //private bool CreateCollection( IEnumerable<Node> nodes )
        //{
        //  //Code for creating a collection of nodes in the software

        //  foreach ( Node node in nodes )
        //  {
        //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
        //    object nodeId = node.CustomData[ AdapterId ];
        //  }

        //  throw new NotImplementedException();
        //}

        ///***************************************************/

        //private bool CreateCollection( IEnumerable<ISectionProperty> sectionProperties )
        //{
        //  //Code for creating a collection of section properties in the software

        //  foreach ( ISectionProperty sectionProperty in sectionProperties )
        //  {
        //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
        //    object secPropId = sectionProperty.CustomData[ AdapterId ];
        //    //If also the default implmentation for the DependencyTypes is used,
        //    //one can from here get the id's of the subobjects by calling (cast into applicable type used by the software): 
        //    object materialId = sectionProperty.Material.CustomData[ AdapterId ];
        //  }

        //  throw new NotImplementedException();
        //}

        ///***************************************************/

        //private bool CreateCollection( IEnumerable<Material> materials )
        //{
        //  //Code for creating a collection of materials in the software

        //  foreach ( Material material in materials )
        //  {
        //    //Tip: if the NextId method has been implemented you can get the id to be used for the creation out as (cast into applicable type used by the software):
        //    object materialId = material.CustomData[ AdapterId ];
        //  }

        //  throw new NotImplementedException();
        //}


        /***************************************************/
    }
}
