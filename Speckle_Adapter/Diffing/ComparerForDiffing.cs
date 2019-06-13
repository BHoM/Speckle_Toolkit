using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.Speckle.Types
{
    public class IBHoMGUIDComparer : IEqualityComparer<IBHoMObject>
    {
        public bool Equals(IBHoMObject x, IBHoMObject y)
        {
            return x.BHoM_Guid == y.BHoM_Guid;
        }

        public int GetHashCode(IBHoMObject obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
