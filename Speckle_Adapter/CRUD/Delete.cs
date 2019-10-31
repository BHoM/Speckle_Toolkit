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

        protected override int Delete(Type type, IEnumerable<object> ids)
        {
            IEnumerable<string> speckleGuids = ids as IEnumerable<string>;

            if (speckleGuids != null)
                DeleteIBHoMObjectsBySpeckleGUID(speckleGuids);

            return 0;
        }

        protected bool DeleteIBHoMObjectsBySpeckleGUID(IEnumerable<string> speckleGuids)
        {
            throw new NotImplementedException();
        }

        /***************************************************/
    }
}
