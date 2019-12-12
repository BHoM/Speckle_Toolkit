using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        public static List<string> SpeckleIds(this FilterRequest request)
        {
            List<string> speckleGuids = new List<string>();

            IList objectIds = null;
            object idObject;
            if (request.Equalities.TryGetValue("ObjectIds", out idObject) && idObject is IList)
                objectIds = idObject as IList;

            if (objectIds == null)
                return speckleGuids;

            for (int i = 0; i < objectIds.Count; i++)
                speckleGuids.Add(objectIds[i].ToString());

            return speckleGuids;
        }
    }
}
