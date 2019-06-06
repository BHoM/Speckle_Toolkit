﻿using BH.oM;
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
            IEnumerable<object> newObjects = (IEnumerable<object>) objects; //hacky; assumes T is always a reference type. Should be no problem anyway

            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(newObjects);
            SpeckleLayerDefault.ObjectCount += objects.Count();
            SpeckleStreamDefault.Objects.AddRange(objs_serialized);

            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleClient.Stream.StreamId, SpeckleStreamDefault).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleClient.Stream.StreamId, new { eventType = "update-global" });

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
