using System;
using System.Collections.Generic;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /***************************************************/
        /**** BHoM Adapter Interface                    ****/
        /***************************************************/

        //Standard implementation of the comparer class.
        //Compares nodes by distance (down to 3 decimal places -> mm)
        //Compares Materials, SectionProprties, LinkConstraints, and Property2D by name
        //Add/remove any type in the dictionary below that you want (or not) a specific comparison method for

        protected override IEqualityComparer<T> Comparer<T>()
        {

            return EqualityComparer<T>.Default;

            //Type type = typeof(T);

            //if (m_Comparers.ContainsKey(type))
            //{
            //    return m_Comparers[type] as IEqualityComparer<T>;
            //}
            //else
            //{
            //    return EqualityComparer<T>.Default;
            //}

        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        //private static Dictionary<Type, object> m_Comparers = new Dictionary<Type, object>
        //    {
        //        {typeof(Node), new BH.Engine.Structure.NodeDistanceComparer(3) },   //The 3 in here sets how many decimal places to look at for node merging. 3 decimal places gives mm precision
        //        {typeof(ISectionProperty), new BHoMObjectNameOrToStringComparer() },
        //        {typeof(Material), new BHoMObjectNameComparer() },
        //        {typeof(LinkConstraint), new BHoMObjectNameComparer() },
        //        {typeof(Property2D), new BHoMObjectNameComparer() },
        //    };


        /***************************************************/
    }
}