using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveResults.Model
{
    class CompetitionData
    {
        public Dictionary<string, List<SplitControl>> splitcontrols { get; set; }
        public List<RunnerAliases> runneraliases { get; set; }
        public List<CompResults> results { get; set; }

    }
}
