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
        /// The duration of each backtest. The time between the start and end dates is divided into equal date range segments
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DurationHours { get; set; }

        /// <summary>
        /// The duration of each backtest. The time between the start and end dates is divided into equal date range segments
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DurationDays { get; set; }

        /// <summary>
        /// If true optimal parameters from an execution will become the starting gene values of the next execution. If false, the gene will use the same gene configuration for each date range
        /// </summary>
        public bool WalkForward { get; set; }


    }
}
