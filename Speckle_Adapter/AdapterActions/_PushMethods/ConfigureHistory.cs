using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        private void configureHistory(Dictionary<string, object> config = null)
        {
            ResponseStreamClone response = null;
            Task<ResponseStreamClone> respStreamClTask = null;

            object enableHistoryObj = null;

            if (config != null)
                config.TryGetValue("EnableHistory", out enableHistoryObj);

            bool? enableHistory = enableHistoryObj as bool?;
            if (enableHistory != null && !(bool)enableHistory)
                return;

            // The following line creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.
            // The streamId of the cloned "backup" is saved among the main Stream "children" field,
            // accessible through SpeckleServerAddress/api/v1/streams/streamId
            respStreamClTask = SpeckleClient.StreamCloneAsync(SpeckleStreamId);

            try
            {
                response = respStreamClTask?.Result;
            }
            catch (Exception e) { }

            if (response == null)
                BH.Engine.Reflection.Compute.RecordWarning($"Could not set the EnableHistory option. Task status: {respStreamClTask.Status.ToString()}");
        }
    }
}