using SMPSOsimulation.dataStructures;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPSOsimulation
{
    public class SMPSOOrchestrator
    {
        public class PositionWithResult
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

        public class Particle
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

        private List<CPUConfig> GetConfigsFromSwarm(List<Particle> swarm)
        {
            return swarm.Select(particle => CPUConfig.GetConfigFromVectorDouble(particle.positionWithResult.position)).ToList();
        }

        private bool TruePositionAlreadyIn(double[] position, List<double[]> destinationList)
        {
            foreach (var destination in destinationList)
            {
                if (CPUConfig.GetConfigFromVectorDouble(position).Equals(CPUConfig.GetConfigFromVectorDouble(destination)))
                    return true;
            }

            return false;
        }

        private bool TruePositionOfParticleAlreadyInParticles(Particle particleToCheck, List<Particle> particles)
        {
            return TruePositionAlreadyIn(particleToCheck.positionWithResult.position,
                particles.Select(particle => particle.positionWithResult.position).ToList());
        }

        private List<double> GetCrowdingDistances(List<PositionWithResult> leaders)
        {
            List<double> crowdingDistances = new List<double>();
            if (leaders.Count == 0)
                return crowdingDistances;

            foreach (var leader in leaders)
            {
                crowdingDistances.Add(0);
            }

            if (leaders.Count == 1)
            {
                crowdingDistances[0] = double.PositiveInfinity;
                return crowdingDistances;
            }
            if (leaders.Count == 2)
            {
                crowdingDistances[0] = double.PositiveInfinity;
                crowdingDistances[1] = double.PositiveInfinity;
                return crowdingDistances;
            }

            // if using reduction of multi objectives to one
            // then leaders archive will contain elements with same score
            // so all of them get +infinity
            if (leaders[0].result.Length == 1)
            {
                for (int i = 0; i < crowdingDistances.Count; i++)
                    crowdingDistances [i] = double.PositiveInfinity;
                return crowdingDistances;
            }

            for (int objective = 0; objective < leaders[0].result.Length; objective++)
            {
                var leadersOrderedByObjective = leaders.OrderBy(leader => leader.result[objective]).ToList();
                var objMinIndex = leadersOrderedByObjective[0];
                var objMaxIndex = leadersOrderedByObjective[leadersOrderedByObjective.Count - 1];
                crowdingDistances[leaders.IndexOf(objMinIndex)] = double.PositiveInfinity;
                crowdingDistances[leaders.IndexOf(objMaxIndex)] = double.PositiveInfinity;
                double objMin = leadersOrderedByObjective[0].result[objective];
                double objMax = leadersOrderedByObjective[leadersOrderedByObjective.Count - 1].result[objective];

                for (int i = 1; i < leadersOrderedByObjective.Count - 1; i++)
                {
                    double distance = leadersOrderedByObjective[i + 1].result[objective] - leadersOrderedByObjective[i - 1].result[objective];
                    distance = distance / (objMax - objMin);
                    crowdingDistances[leaders.IndexOf(leadersOrderedByObjective[i])] += distance;
                }
            }

            return crowdingDistances;
        }


        public List<(CPUConfig, double[])> StartSearch(SearchConfigSMPSO searchConfig, string psatsimExePath, string gtkLibPath)
        {
            Random random = new Random();

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

            // init swarm
            List<Particle> swarm = new List<Particle>();
            for (int i = 0; i < searchConfig.swarmSize; i++)
            {
                var randomPositionWithResult = new PositionWithResult(CPUConfig.GenerateRandom(searchConfig.environment.MaxFrequency), [double.PositiveInfinity, double.PositiveInfinity]);
                var speed = new double[16];
                swarm.Add(new Particle(randomPositionWithResult, speed, randomPositionWithResult));
            }

            var results = resultsProvider.Evaluate(GetConfigsFromSwarm(swarm));

            for (int i = 0; i < swarm.Count; i++)
            {
                var particle = swarm[i];
                particle.positionWithResult.result = results[i];
                particle.personalBest.result = results[i];
            }

            // init leaders archive
            List<PositionWithResult> leadersArchive = new List<PositionWithResult>(searchConfig.archiveSize);
            foreach (var particle in swarm)
            {
                if (domination.IsDominated(particle, swarm) == false &&
                    TruePositionOfParticleAlreadyInParticles(particle, swarm) == false)
                    leadersArchive.Add(particle.positionWithResult);
            }
            List<double> crowdingDistances = GetCrowdingDistances(leadersArchive);

            int generation = 0;
            while (generation < searchConfig.maxGenerations)
            {
                // update speeds of particles
                foreach (var particle in swarm)
                {
                    double r1 = random.NextDouble();
                    double r2 = random.NextDouble();

                    var xi = particle.positionWithResult.position;
                    var xp = particle.personalBest.position;

                    var l1Index = random.Next(leadersArchive.Count);
                    var l2Index = random.Next(leadersArchive.Count);
                    var l1 = leadersArchive[l1Index];
                    var l2 = leadersArchive[l2Index];

                    var crowd1 = crowdingDistances[l1Index];
                    var crowd2 = crowdingDistances[l2Index];
                    var xg = crowd1 > crowd2 ? l1.position : l2.position;

                    double c1 = 1.5 + random.NextDouble();
                    double c2 = 1.5 + random.NextDouble();
                    double w = 0.1;

                    particle.speed = VectorTools.AddArrays(
                        VectorTools.Multiply(w, particle.speed),
                        VectorTools.Multiply(c1 * r1, VectorTools.AddArrays(xp, VectorTools.Multiply(-1, xi))),
                        VectorTools.Multiply(c2 * r2, VectorTools.AddArrays(xg, VectorTools.Multiply(-1, xi)))
                    );

                    double phi = c1 + c2 > 4 ? c1 + c2 : 0;
                    double rho = 2 / (2 - phi - Math.Sqrt(phi * phi - 4 * phi));
                    particle.speed = VectorTools.Multiply(rho, particle.speed);

                    for (int i = 0; i < particle.speed.Length; i++)
                    {
                        var minLimit = CPUConfig.CPUConfigLimits.GetMin(i);
                        var maxLimit = CPUConfig.CPUConfigLimits.GetMax(i, searchConfig.environment.MaxFrequency);
                        double delta = (maxLimit - minLimit) / 2.0;

                        if (particle.speed[i] > delta) particle.speed[i] = delta;
                        if (particle.speed[i] <= -delta) particle.speed[i] = -delta;
                    }
                }

                // update positions of particles
                foreach (var particle in swarm)
                {
                    particle.positionWithResult.position = VectorTools.AddArrays(
                        particle.positionWithResult.position,
                        particle.speed
                    );

                    for (int i = 0; i < particle.positionWithResult.position.Length; i++)
                    {

                    }
                }
            }
        }
    }
}
