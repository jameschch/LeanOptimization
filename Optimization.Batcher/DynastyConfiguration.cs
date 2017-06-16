using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Batcher
{

    /// <summary>
    /// A Dynasty is a sequence of generations.
    /// </summary>
    public class DynastyConfiguration
    {

        /// <summary>
        /// The start date of the dynasty. This is the first date of the first set of generations
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the dynasty. This is the last date of the last generations
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The duration of each backtest. The time between the start and end dates is divided into equal date range periods
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DurationHours { get; set; }

        /// <summary>
        /// The duration of each backtest. The time between the start and end dates is divided into equal date range periods
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DurationDays { get; set; }

        /// <summary>
        /// If set to true, the optimal parameters from an execution will become the starting gene values of the next execution.
        /// If false, the gene will use the same gene configuration for each date period
        /// </summary>
        public bool WalkForward { get; set; }

        /// <summary>
        /// Allows specification of unequal time segments for each time period. These time segments will be cycled in order until the end date is reached
        /// </summary>
        public int[] HourSegments { get; set; }
    }
}
