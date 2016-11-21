using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    [Serializable]
    public class Variables
    {
        public Dictionary<string, object> vars = new Dictionary<string, object>();
        public override bool Equals(object obj)
        {
            var item = obj as Variables;
            return Equals(item);
        }

        protected bool Equals(Variables other)
        {
            foreach (KeyValuePair<string, object> kvp in vars)
            {
                if (kvp.Value.ToString() != other.vars[kvp.Key].ToString())
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                foreach (KeyValuePair<string, object> kvp in vars)
                    hashCode = hashCode * kvp.Value.GetHashCode();
                return hashCode;
            }
        }
    }
}
