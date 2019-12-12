using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.Engine.Speckle;


namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            // Not used. Override required due to `abstract` in base adapter. To be removed after Refactoring Level 04.
            return null;
        }


        protected IEnumerable<object> ReadAll(bool assignSpeckleIdToBHoMObjects = true)
        {
            var response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            if (!BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects))
                BH.Engine.Reflection.Compute.RecordError("Failed to deserialize and cast the Server response into BHoM objects.");

            return bHoMObjects.Concat(iObjects).Concat(reminder);
        }

        protected bool Read(FilterRequest filterRequest, out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder)
        {
            var response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            // Extract the speckleIds for selection from the FilterRequest.
            List<string> speckleIds = Query.SpeckleIds(filterRequest);
            speckleIds = speckleIds?.Count != 0 ? speckleIds : null;

            // Convert the response to the appropriate object types.
            BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, speckleIds?.Count != 0, speckleIds);

            // Filter by tag if any 
            bHoMObjects = filterRequest.Tag == "" ? bHoMObjects : bHoMObjects.Where(x => x.Tags.Contains(filterRequest.Tag)).ToList();

            return true;
        }
    }
}
