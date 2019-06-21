using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            // // - This method will only be called from the BHoM_Adapter by either: 1) The Replace method 2) The Read overload with the query stuff in it

            ////Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            //if (type == typeof(Node))
            //    return ReadNodes(ids as dynamic);
            //else if (type == typeof(Bar))
            //    return ReadBars(ids as dynamic);
            //else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
            //    return ReadSectionProperties(ids as dynamic);
            //else if (type == typeof(Material))
            //    return ReadMaterials(ids as dynamic);

            ///------
            ///

            List<string> speckleGuids = new List<string>();

            for (int i = 0; i < ids?.Count; i++)
                speckleGuids.Add(ids[i].ToString());

            ResponseObject response = MakeGETRequest(speckleGuids);               
            
            List <IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            bool assignSpeckleIdToBHoMObjects = true;

            if (!BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects))
            {
                Engine.Reflection.Compute.RecordError("Failed to elaborate server response.");
                return new List<IBHoMObject>();
            }

            return bHoMObjects;
        }

  
        public ResponseObject MakeGETRequest(List<string> speckleGuids = null)
        {
            if (speckleGuids == null)
            {
                return SpeckleClient.StreamGetObjectsAsync(SpeckleClient.Stream.StreamId, "").Result;      
            }
            else
            {
                // GET only specific IDs
                return SpeckleClient.ObjectGetBulkAsync(speckleGuids.ToArray(), "omit=displayValue").Result;
            }
        }

        public List<string> QueryToSpeckleIds(FilterRequest query)
        {
            List<string> speckleGuids = new List<string>();

            IList objectIds = null;
            object idObject;
            if (query.Equalities.TryGetValue("ObjectIds", out idObject) && idObject is IList)
                objectIds = idObject as IList;

            if (objectIds == null)
                return speckleGuids;

            for (int i = 0; i < objectIds.Count; i++)
                speckleGuids.Add(objectIds[i].ToString());

            return speckleGuids;
        }

        /***************************************************/
        /**** Private specific read methods             ****/
        ///***************************************************/

        ////The List<string> in the methods below can be changed to a list of any type of identification more suitable for the toolkit

        //private List<Bar> ReadBars( List<string> ids = null )
        //{
        //  //Implement code for reading bars
        //  throw new NotImplementedException();
        //}

        ///***************************************/

        //private List<Node> ReadNodes( List<string> ids = null )
        //{
        //  //Implement code for reading nodes
        //  throw new NotImplementedException();
        //}

        ///***************************************/

        //private List<ISectionProperty> ReadSectionProperties( List<string> ids = null )
        //{
        //  //Implement code for reading section properties
        //  throw new NotImplementedException();
        //}

        ///***************************************/

        //private List<Material> ReadMaterials( List<string> ids = null )
        //{
        //  //Implement code for reading materials
        //  throw new NotImplementedException();
        //}

        /***************************************************/


    }
}
