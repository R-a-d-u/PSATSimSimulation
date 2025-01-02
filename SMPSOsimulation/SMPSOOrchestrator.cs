using SMPSOsimulation.dataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPSOsimulation
{
    public class SMPSOOrchestrator
    {
        private class PositionWithResult
        {
            public double[] position; // size 16
            public double[] result; // size 2: 0 - CPI, 1 - Energy

            public PositionWithResult(double[] position, double[] result)
            {
                this.position = position;
                this.result = result;
            }

            public PositionWithResult(CPUConfig config, double[] result)
            {
                this.position = config.GetVectorFormDouble();
                this.result = result;
            }

            public CPUConfig GetConfigFromPosition()
            {
                return CPUConfig.GetConfigFromVectorDouble(position);
            }
        }

        private class Particle
        {
            public PositionWithResult positionWithResult;
            public double[] speed; // size 16
            public PositionWithResult personalBest;

            public Particle(PositionWithResult positionWithResult, double[] speed, PositionWithResult personalBest)
            {
                this.positionWithResult = positionWithResult;
                this.speed = speed;
                this.personalBest = personalBest;
            }
        }

        public List<(CPUConfig, double[])> StartSearch(SearchConfigSMPSO searchConfig, string psatsimExePath, string gtkLibPath)
        {
            ResultsProvider resultsProvider = new ResultsProvider(searchConfig.environment, psatsimExePath, gtkLibPath);
            DominationProvider domination;
            switch (searchConfig.domination.dominationType)
            {
                case DominationConfig.DominationType.SMPSO:
                    domination = new SMPSODomination();
                    break;
                case DominationConfig.DominationType.WEIGHT:
                    domination = new WeightedSumDomination(searchConfig.domination.wCPI!.Value, searchConfig.domination.wEnergy!.Value);
                    break;
                case DominationConfig.DominationType.LEXICOGRAPHIC:
                    domination = new LexicographicDomination(searchConfig.domination.prefferedObjective!.Value, searchConfig.domination.tolerance!.Value);
                    break;
                default:
                    throw new Exception("StartSearch init of domination provider; how did we get here??");
            }

            List<Particle> swarm = new List<Particle>();
            for (int i = 0; i < searchConfig.swarmSize; i++)
            {
                var randomPositionWithResult = new PositionWithResult(CPUConfig.GenerateRandom(searchConfig.environment.MaxFrequency), [double.PositiveInfinity, double.PositiveInfinity]);
                var speed = new double[16];
                swarm.Add(new Particle(randomPositionWithResult, speed, randomPositionWithResult));
            }
        }
    }
}
