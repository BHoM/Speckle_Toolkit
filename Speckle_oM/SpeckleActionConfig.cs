using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Speckle
{
    public class SpeckleActionConfig : BH.oM.Adapter.ActionConfig
    {
        public bool SetAssignedId { get; set; } = true;

        public bool EnableHistory { get; set; } = true;

        public bool UseGUIDS { get; set; } = false;
    }
}
