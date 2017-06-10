using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{
    public class FixedSizeQueue<T> : Queue<T>
    {

        public FixedSizeQueue(int limit)
        {
            _limit = limit;
        }

        private int _limit { get; set; }
        public new void Enqueue(T obj)
        {
            while (this.Count > _limit)
            {
                this.Dequeue();
            }
            base.Enqueue(obj);
        }
    }

}
