using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.Utils
{
    public class IgnoreCaseStringCompare<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if(x==null || y == null)
            {
                if (x == null && y == null)
                    return true;

                return false;
            }

           return x.ToString().Equals(y.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
