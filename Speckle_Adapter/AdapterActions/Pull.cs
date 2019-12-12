using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.Engine.Speckle;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public override IEnumerable<object> Pull(IRequest request, Dictionary<string, object> config = null)
        {
            if (request == null)
                return ReadAll();

            // Make sure this is a FilterRequest
            FilterRequest filterRequest = request as FilterRequest;
            if (filterRequest == null)
            {
                Engine.Reflection.Compute.RecordWarning("Please specify a FilterRequest");
                return new List<object>();
            }

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            Read(filterRequest, out bHoMObjects, out iObjects, out reminder);

            // Return stuff
            if (typeof(IBHoMObject).IsAssignableFrom(filterRequest.Type))
                return bHoMObjects;
            else if (typeof(IObject).IsAssignableFrom(filterRequest.Type))
                return iObjects;
            else
                return bHoMObjects.Concat(iObjects).Concat(reminder);
        }
    }
}
