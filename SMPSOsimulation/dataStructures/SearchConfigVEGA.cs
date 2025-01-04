namespace SMPSOsimulation.dataStructures;

public class SearchConfigVEGA(int maxGenerations, int populationSize, double mutationProbability, int maxFrequency, EnvironmentConfig environment)
{
    internal readonly int MaxFrequency = maxFrequency;
    public int maxGenerations = maxGenerations;
    public int populationSize = populationSize;
    public double mutationProbability = mutationProbability;
    public EnvironmentConfig environment = environment;
}