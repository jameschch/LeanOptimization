using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{
    public class FixedSizeQueue<T> : Queue<T>
    {

        private int _limit;

        public FixedSizeQueue(int limit)
        {
            _limit = limit;
        }

        public new void Enqueue(T obj)
        {
            while (Count > _limit)
            {
                Dequeue();
            }
            base.Enqueue(obj);
        }
    }

}
