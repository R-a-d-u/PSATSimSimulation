using System.Security.Cryptography;
using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation;

public class VEGAOrchestrator
{
    private class Individual
    {
        public int[] genes;
        public double[] result;

        public Individual(CPUConfig config)
        {
            genes = new int[]
            {
                config.Superscalar,
                config.Rename,
                config.Reorder,
                (int)config.RsbArchitecture,
                config.SeparateDispatch ? 1 : 0,
                config.Iadd,
                config.Imult,
                config.Idiv,
                config.Fpadd,
                config.Fpmult,
                config.Fpdiv,
                config.Fpsqrt,
                config.Branch,
                config.Load,
                config.Store,
                config.Freq
            };
            result = new double[2];
        }

        public CPUConfig GetConfig()
        {
            // Map the genes back to a CPUConfig
            int superscalar = genes[0];
            int rename = genes[1];
            int reorder = genes[2];
            RsbArchitectureType rsbArchitecture = (RsbArchitectureType)genes[3];
            bool separateDispatch = genes[4] == 1; // Convert back to boolean
            int iadd = genes[5];
            int imult = genes[6];
            int idiv = genes[7];
            int fpadd = genes[8];
            int fpmult = genes[9];
            int fpdiv = genes[10];
            int fpsqrt = genes[11];
            int branch = genes[12];
            int load = genes[13];
            int store = genes[14];
            int freq = genes[15];

            // Create and return a new CPUConfig object
            return new CPUConfig(superscalar, rename, reorder, rsbArchitecture, separateDispatch,
                iadd, imult, idiv, fpadd, fpmult, fpdiv, fpsqrt, branch, load, store, freq);
        }
    }

    private (Individual, Individual) Crossover(Individual parent1, Individual parent2, float maxFrequency)
    {
        // Constants for SBX (can be adjusted)
        double eta = 15.0; // Distribution index (controls the spread of the offspring around the parents)
        Random random = new Random();

        // Create offspring
        Individual child1 = new Individual(parent1.GetConfig());
        Individual child2 = new Individual(parent2.GetConfig());

        for (int i = 0; i < parent1.genes.Length; i++)
        {
            // Extract the parent genes as doubles for calculation
            double gene1 = parent1.genes[i];
            double gene2 = parent2.genes[i];

            // Swap the values if necessary to ensure gene1 is the smaller value
            if (random.NextDouble() < 0.5)
            {
                (gene1, gene2) = (gene2, gene1);
            }

            // Apply SBX
            double delta = gene2 - gene1;
            double u = random.NextDouble();

            double beta;
            if (u <= 0.5)
            {
                beta = Math.Pow((2 * u), (1.0 / (eta + 1.0)));
            }
            else
            {
                beta = Math.Pow((0.5 / (1 - u)), (1.0 / (eta + 1.0)));
            }

            double childGene1 = 0.5 * ((gene1 + gene2) - beta * delta);
            double childGene2 = 0.5 * ((gene1 + gene2) + beta * delta);

            // Convert the continuous offspring genes back to integers
            if (i == 3)
            {
                childGene1 = Math.Max(Math.Min(childGene1, Enum.GetValues(typeof(RsbArchitectureType)).Length - 1), 0);
                childGene2 = Math.Max(Math.Min(childGene2, Enum.GetValues(typeof(RsbArchitectureType)).Length - 1), 0);
            }
            else if (i == 4)
            {
                childGene1 = Math.Max(Math.Min(childGene1, 1), 0);
                childGene2 = Math.Max(Math.Min(childGene2, 1), 0);
            }
            else if (i == 15)
            {
                childGene1 = Math.Max(Math.Min(childGene1, maxFrequency), 0);
                childGene2 = Math.Max(Math.Min(childGene2, maxFrequency), 0);
            }
            else
            {
                childGene1 = Math.Max(Math.Min(childGene1, CPUConfig.CPUConfigLimits.GetMax(i)), CPUConfig.CPUConfigLimits.GetMin(i));
                childGene2 = Math.Max(Math.Min(childGene2, CPUConfig.CPUConfigLimits.GetMax(i)), CPUConfig.CPUConfigLimits.GetMin(i));
            }
            child1.genes[i] = (int)Math.Round(childGene1);
            child2.genes[i] = (int)Math.Round(childGene2);
        }

        return (child1, child2);
    }


    private void Mutate(Individual individual, Random random, double mutationProbability, int maxFrequency)
    {
        // Loop through each gene in the individual
        for (int i = 0; i < individual.genes.Length; i++)
        {
            // For each gene, decide whether it should mutate based on the mutation probability
            if (random.NextDouble() < mutationProbability)
            {
                if (i == 3)
                {
                    individual.genes[i] = random.Next(0, Enum.GetValues(typeof(RsbArchitectureType)).Length);
                }
                else if (i == 4)
                {
                    individual.genes[i] = random.Next(0, 2);
                }
                else if (i == 15)
                {
                    individual.genes[i] = random.Next(1, maxFrequency);
                }
                else
                {
                    individual.genes[i] = random.Next(CPUConfig.CPUConfigLimits.GetMin(i), CPUConfig.CPUConfigLimits.GetMax(i) + 1);
                }
            }
        }
    }
    
    private static void Shuffle<T>(List<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1); // Generate a random index between 0 and i
            // Swap elements at indices i and j
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public List<CPUConfig> StartSearch(SearchConfigVEGA searchConfig, string psatsimExePath, string gtkLibPath)
    {
        ResultsProvider resultsProvider = new ResultsProvider(searchConfig.environment, psatsimExePath, gtkLibPath);
        int subPopSize = searchConfig.populationSize / 2;
        List<Individual> population = new List<Individual>();
        Random random = new Random();
        for (int i = 0; i < searchConfig.populationSize; i++)
            population.Add(new Individual(CPUConfig.GenerateRandom(searchConfig.environment.MaxFrequency)));
        
        for (int generation = 0; generation < searchConfig.maxGenerations; generation++)
        {
            //DEbug
            Console.WriteLine("Generation: " + generation);
            foreach (Individual individual in population)
            {
                Console.WriteLine($"{individual.result[0]} {individual.result[1]}");
            }
            
            List<Individual> children = new List<Individual>();
            while (population.Count < searchConfig.populationSize * 3 / 2)
            {
                int index1 = random.Next(population.Count);
                int index2;

                // Ensure the second index is different from the first
                do
                {
                    index2 = random.Next(population.Count);
                } while (index2 == index1);

                var parent1 = population[index1];
                var parent2 = population[index2];
                
                var (child1, child2) = Crossover(parent1, parent2, searchConfig.environment.MaxFrequency);
                
                Mutate(child1, random, searchConfig.mutationProbability, searchConfig.environment.MaxFrequency);
                Mutate(child2, random, searchConfig.mutationProbability, searchConfig.environment.MaxFrequency);
                
                children.Add(child1);
                children.Add(child2);
            }
            
            foreach (var child in children)
                population.Add(child);
            
            Shuffle(population, random);
            
            List<CPUConfig> individualsConfigs = new List<CPUConfig>();
            foreach (var individual in population)
                individualsConfigs.Add(individual.GetConfig());
            var results = resultsProvider.Evaluate(individualsConfigs);
            for (int i = 0; i < population.Count; i++)
            {
                population[i].result = results[i];
            }

            List<Individual> bestCPI = population
                .OrderBy(ind => ind.result[0]) // Sort by CPI (result[0]) in ascending order
                .Take(subPopSize) // Take the top 'subPopSize' individuals
                .ToList();
        
            List<Individual> bestEnergy = population
                .OrderBy(ind => ind.result[1]) // Sort by CPI (result[0]) in ascending order
                .Take(subPopSize) // Take the top 'subPopSize' individuals
                .ToList();

            population = bestCPI
                .Union(bestEnergy) // Get the union of bestCPI and bestEnergy
                .Distinct() // Ensure no duplicates in case an individual is in both lists
                .ToList(); // Convert back to a list
        }
        
        List<CPUConfig> returnedConfigs = new List<CPUConfig>();
        foreach (var individual in population)
            returnedConfigs.Add(individual.GetConfig());

        return returnedConfigs;
    }
}