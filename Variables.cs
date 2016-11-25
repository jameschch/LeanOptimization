using GAF;
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
        public Dictionary<string, object> Items = new Dictionary<string, object>();
        public override bool Equals(object obj)
        {
            var item = obj as Variables;
            return Equals(item);
        }

        protected bool Equals(Variables other)
        {
            foreach (KeyValuePair<string, object> kvp in Items)
            {
                if (kvp.Value.ToString() != other.Items[kvp.Key].ToString())
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                foreach (KeyValuePair<string, object> kvp in Items)
                    hashCode = hashCode * kvp.Value.GetHashCode();
                return hashCode;
            }
        }

        public static Variables SpawnRandom()
        {
            Variables v = new Variables();
            v.Items = new Dictionary<string, object>
                    {
                        { "p1", Program.RandomBetween(1.5, 3.25) },
                        { "p2", Program.RandomBetween(0.25, 3.25) },
                        { "p3", Program.RandomBetween(1, 12) },
                        { "p4", Program.RandomBetween(1, 12) },
                        { "stop", Program.RandomBetween(0.01, 0.05) },
                        { "take", Program.RandomBetween(0.02, 0.08) },
                    };

            return v;
        }

        public static Chromosome Spawn()
        {
            var chromosome = new Chromosome();
            var spawn = Variables.SpawnRandom();

            foreach (var item in spawn.Items)
            {
                chromosome.Genes.Add(new Gene(item));
            }

            return chromosome;
        }

    }
}
