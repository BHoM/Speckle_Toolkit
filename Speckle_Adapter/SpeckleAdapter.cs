using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public SpeckleAdapter(SpeckleCore.Account speckleAccount, string speckleStreamId)
        {
            m_AdapterSettings.UseAdapterId = false;

            AdapterIdName = BH.Engine.Speckle.Convert.AdapterIdName;

            SpeckleAccount = speckleAccount;
            SpeckleStream = new SpeckleStream() { StreamId = SpeckleStreamId };

            SpeckleClient = new SpeckleApiClient() { BaseUrl = SpeckleAccount.RestApi, AuthToken = SpeckleAccount.Token, Stream = SpeckleStream }; // hacky, but i don't want to rebuild stuff and fiddle dll loading etc.
            SpeckleClient.SetupWebsocket();

            SpeckleStreamId = speckleStreamId;
        }


        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public SpeckleApiClient SpeckleClient { get; private set; }
        public string SpeckleStreamId { get; private set; }
        public Account SpeckleAccount { get; private set; }
        public SpeckleStream SpeckleStream { get; private set; }
        public SpeckleCore.Layer SpeckleLayer { get; private set; }

    }
}
