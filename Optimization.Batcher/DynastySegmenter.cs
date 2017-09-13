using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{

    public class DynastySegmenter
    {

        private DynastyConfiguration _config;
        private Queue<int> _segments;

        public DynastySegmenter(DynastyConfiguration config)
        {
            _config = config;
        }

        public DateTime PeekNext(DateTime i)
        {
            return GetNext(i, true);
        }

        public DateTime GetNext(DateTime i, bool isPeeking = false)
        {
            if (_config.DurationDays == 0 && _config.DurationHours == 0 && !_config.HourSegments.Any())
            {
                throw new ArgumentException("Duration must be specified");
            }

            if (_config.HourSegments != null && _config.HourSegments.Any(h => h > 0))
            {
                if (_segments == null || !_segments.Any())
                {
                    _segments = new Queue<int>(_config.HourSegments);
                }

                if (isPeeking)
                {
                    return i.AddHours(_segments.Peek());
                }

                return i.AddHours(_segments.Dequeue());
            }
            else
            {
                return i.AddDays(_config.DurationDays).AddHours(_config.DurationHours);
            }
        }

    }
}
