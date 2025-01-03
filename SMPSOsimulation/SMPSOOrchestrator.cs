using SMPSOsimulation.dataStructures;

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

            public PositionWithResult Copy()
            {
                double[] copyPosition = new double[this.position.Length];
                double[] copyResult = new double[this.result.Length];

                for (int i = 0; i < this.position.Length; i++)
                {
                    copyPosition[i] = this.position[i];
                }

                for (int i = 0; i < this.result.Length; i++)
                {
                    copyResult[i] = this.result[i];
                }

                return new PositionWithResult(copyPosition, copyResult);
            }

            public CPUConfig GetConfigFromPosition()
            {
                return CPUConfig.GetConfigFromVectorDouble(position);
            }
        }

        public class Particle(SMPSOOrchestrator.PositionWithResult positionWithResult, double[] speed, SMPSOOrchestrator.PositionWithResult personalBest)
        {
            public PositionWithResult positionWithResult = positionWithResult;
            public double[] speed = speed; // size 16
            public PositionWithResult personalBest = personalBest;
        }

        private static List<CPUConfig> GetConfigsFromSwarm(List<Particle> swarm)
        {
            return swarm.Select(particle => CPUConfig.GetConfigFromVectorDouble(particle.positionWithResult.position)).ToList();
        }

        private static bool TruePositionAlreadyIn(double[] position, List<double[]> destinationList)
        {
            foreach (var destination in destinationList)
            {
                if (CPUConfig.GetConfigFromVectorDouble(position).Equals(CPUConfig.GetConfigFromVectorDouble(destination)))
                    return true;
            }

            return false;
        }

        private static bool TruePositionOfParticleAlreadyInLeaders(Particle particleToCheck, List<PositionWithResult> leaders)
        {
            return TruePositionAlreadyIn(particleToCheck.positionWithResult.position, leaders.Select(leader => leader.position).ToList());
        }

        private static bool TrueLeaderAlreadyInLeaders(PositionWithResult particleToCheck, List<PositionWithResult> leaders)
        {
            return TruePositionAlreadyIn(particleToCheck.position, leaders.Select(leader => leader.position).ToList());
        }

        private static List<double> GetCrowdingDistances(List<PositionWithResult> leaders)
        {
            List<double> crowdingDistances = [];
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
                    crowdingDistances[i] = double.PositiveInfinity;
                return crowdingDistances;
            }

            for (int objective = 0; objective < leaders[0].result.Length; objective++)
            {
                var leadersOrderedByObjective = leaders.OrderBy(leader => leader.result[objective]).ToList();
                var objMinIndex = leadersOrderedByObjective[0];
                var objMaxIndex = leadersOrderedByObjective[^1];
                crowdingDistances[leaders.IndexOf(objMinIndex)] = double.PositiveInfinity;
                crowdingDistances[leaders.IndexOf(objMaxIndex)] = double.PositiveInfinity;
                double objMin = leadersOrderedByObjective[0].result[objective];
                double objMax = leadersOrderedByObjective[^1].result[objective];

                for (int i = 1; i < leadersOrderedByObjective.Count - 1; i++)
                {
                    double distance = leadersOrderedByObjective[i + 1].result[objective] - leadersOrderedByObjective[i - 1].result[objective];
                    distance /= (objMax - objMin);
                    crowdingDistances[leaders.IndexOf(leadersOrderedByObjective[i])] += distance;
                }
            }

            return crowdingDistances;
        }

        private static List<Particle> InitSwarm(SearchConfigSMPSO searchConfig, ResultsProvider resultsProvider)
        {
            List<Particle> swarm = [];
            for (int i = 0; i < searchConfig.swarmSize; i++)
            {
                var randomPositionWithResult = new PositionWithResult(CPUConfig.GenerateRandom(searchConfig.MaxFrequency), [double.PositiveInfinity, double.PositiveInfinity]);
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

            return swarm;
        }
        private static List<PositionWithResult> InitLeadersArchive(List<Particle> swarm, SearchConfigSMPSO searchConfig, DominationProvider domination)
        {
            List<PositionWithResult> leadersArchive = new(searchConfig.archiveSize);
            foreach (var particle in swarm)
            {
                if (domination.IsDominated(particle, swarm) == false &&
                    TruePositionOfParticleAlreadyInLeaders(particle, leadersArchive) == false)
                    leadersArchive.Add(particle.positionWithResult.Copy());
            }
            return leadersArchive;
        }

        private static DominationProvider InitDomination(SearchConfigSMPSO searchConfig)
        {
            DominationProvider domination = searchConfig.domination.dominationType switch
            {
                DominationConfig.DominationType.SMPSO => new SMPSODomination(),
                DominationConfig.DominationType.WEIGHT => new WeightedSumDomination(searchConfig.domination.wCPI!.Value, searchConfig.domination.wEnergy!.Value),
                DominationConfig.DominationType.LEXICOGRAPHIC => new LexicographicDomination(searchConfig.domination.prefferedObjective!.Value, searchConfig.domination.tolerance!.Value),
                _ => throw new Exception("StartSearch init of domination provider; how did we get here??"),
            };
            return domination;
        }

        private static void UpdateSwarmSpeeds(List<Particle> swarm, Random random, List<PositionWithResult> leadersArchive, List<double> crowdingDistances, SearchConfigSMPSO searchConfig)
        {
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
                    var maxLimit = CPUConfig.CPUConfigLimits.GetMax(i, searchConfig.MaxFrequency);
                    double delta = (maxLimit - minLimit) / 2.0;

                    if (particle.speed[i] > delta) particle.speed[i] = delta;
                    if (particle.speed[i] <= -delta) particle.speed[i] = -delta;
                }
            }
        }

        private static void UpdateSwarmPositions(List<Particle> swarm, Random random, SearchConfigSMPSO searchConfig)
        {
            foreach (var particle in swarm)
            {
                particle.positionWithResult.position = VectorTools.AddArrays(
                    particle.positionWithResult.position,
                    particle.speed
                );

                for (int i = 0; i < particle.positionWithResult.position.Length; i++)
                {
                    var minLimit = CPUConfig.CPUConfigLimits.GetMin(i);
                    var maxLimit = CPUConfig.CPUConfigLimits.GetMax(i, searchConfig.MaxFrequency);
                    if (particle.positionWithResult.position[i] > maxLimit)
                    {
                        particle.speed[i] *= -1;
                        particle.positionWithResult.position[i] = maxLimit;
                    }
                    else if (particle.positionWithResult.position[i] < minLimit)
                    {
                        particle.speed[i] *= -1;
                        particle.positionWithResult.position[i] = minLimit;
                    }
                }
            }

            // try applying turbulence
            for (int particleIndex = 0; particleIndex < swarm.Count; particleIndex++)
            {
                if (particleIndex % 6 == 0)
                {
                    var particle = swarm[particleIndex];
                    for (int i = 0; i < particle.positionWithResult.position.Length; i++)
                    {
                        var p = random.NextDouble();
                        if (p < searchConfig.turbulenceRate)
                        {
                            particle.positionWithResult.position[i] =
                                JMetalPolynomialTurbulence(particle.positionWithResult.position[i], i,
                                    searchConfig.MaxFrequency, random);
                        }
                    }
                }
            }
        }

        public static void UpdateSwarmResults(List<Particle> swarm, ResultsProvider resultsProvider, DominationProvider domination)
        {
            var results = resultsProvider.Evaluate(GetConfigsFromSwarm(swarm));
            for (int i = 0; i < results.Length; i++)
            {
                swarm[i].positionWithResult.result = results[i];
            }

            foreach (var particle in swarm)
            {
                if (domination.IsDominated(particle.personalBest.result, [particle.positionWithResult.result]))
                    particle.personalBest = particle.positionWithResult;
            }
        }

        public static void UpdateLeadersArchiveAndCrowdingDistances(List<PositionWithResult> leadersArchive, List<double> crowdingDistances, List<Particle> swarm, DominationProvider domination, SearchConfigSMPSO searchConfig)
        {
            List<PositionWithResult> allPositions = new(leadersArchive);
            allPositions.AddRange(swarm.Select(particle => particle.positionWithResult).ToList());
            leadersArchive.Clear();
            foreach (var particle in allPositions)
            {
                if (domination.IsDominated(particle, allPositions) == false &&
                    TrueLeaderAlreadyInLeaders(particle, leadersArchive) == false)
                {
                    leadersArchive.Add(particle.Copy());
                }
            }
            crowdingDistances.Clear();
            crowdingDistances.AddRange(GetCrowdingDistances(leadersArchive));
            if (leadersArchive.Count > searchConfig.archiveSize)
            {
                var sortedLeaders = leadersArchive.OrderBy(leader => crowdingDistances[leadersArchive.IndexOf(leader)]).ToList();
                var prevCount = leadersArchive.Count;
                for (int i = 0; i < prevCount - searchConfig.archiveSize; i++)
                {
                    leadersArchive.Remove(sortedLeaders[i]);
                }
                crowdingDistances.Clear();
                crowdingDistances.AddRange(GetCrowdingDistances(leadersArchive));
            }
        }


        public static List<(CPUConfig, double[])> StartSearch(SearchConfigSMPSO searchConfig, string psatsimExePath, string gtkLibPath, string tracePath)
        {
            Random random = new();
            ResultsProvider resultsProvider = new(searchConfig.environment, psatsimExePath, gtkLibPath, tracePath);
            var domination = InitDomination(searchConfig);
            var swarm = InitSwarm(searchConfig, resultsProvider);
            var leadersArchive = InitLeadersArchive(swarm, searchConfig, domination);
            var crowdingDistances = GetCrowdingDistances(leadersArchive);
            int generation = 0;
            while (generation < searchConfig.maxGenerations)
            {
                UpdateSwarmSpeeds(swarm, random, leadersArchive, crowdingDistances, searchConfig);
                UpdateSwarmPositions(swarm, random, searchConfig);
                UpdateSwarmResults(swarm, resultsProvider, domination);
                UpdateLeadersArchiveAndCrowdingDistances(leadersArchive, crowdingDistances, swarm, domination, searchConfig);
                if (crowdingDistances.Count != leadersArchive.Count)
                {
                    Console.WriteLine($"Crowding distance {crowdingDistances.Count} != leaders archive {leadersArchive.Count}");
                }
                Console.WriteLine("Generation: " + generation);
                foreach (var leader in leadersArchive.OrderBy(leader => leader.result[0]))
                {
                    Console.WriteLine($"{leader.result[0]} {leader.result[1]}");
                }
                generation++;

            }

            return leadersArchive.Select(leader => (leader.GetConfigFromPosition(), leader.result)).ToList();
        }

        private static double JMetalPolynomialTurbulence(double gene, int geneIndex, int maxFrequency, Random random)
        {
            const double eta_m = 20;
            double min = CPUConfig.CPUConfigLimits.GetMin(geneIndex);
            double max = CPUConfig.CPUConfigLimits.GetMax(geneIndex, maxFrequency);
            double delta1 = (gene - min) / (max - min);
            double delta2 = (max - gene) / (max - min);
            double mut_pow = 1.0 / (eta_m + 1.0);
            double rnd = random.NextDouble();
            double deltaq, xy, val;
            if (rnd <= 0.5)
            {
                xy = 1.0 - delta1;
                val = 2.0 * rnd + (1.0 - 2.0 * rnd) * (Math.Pow(xy, (eta_m + 1.0)));
                deltaq = Math.Pow(val, mut_pow) - 1.0;
            }
            else
            {
                xy = 1.0 - delta2;
                val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * (Math.Pow(xy, (eta_m + 1.0)));
                deltaq = 1.0 - (Math.Pow(val, mut_pow));
            }
            gene += deltaq * (max - min);
            if (gene < min)
            {
                gene = min;
            }
            if (gene > max)
            {
                gene = max;
            }

            return gene;
        }
    }
}
