namespace SMPSOsimulation.dataStructures;

public class SearchConfigVEGA
{
    public int maxGenerations;
    public int populationSize;
    public double mutationProbability;
    public EnvironmentConfig environment;

    public SearchConfigVEGA(int maxGenerations, int populationSize, double mutationProbability, EnvironmentConfig environment)
    {
        this.maxGenerations = maxGenerations;
        this.populationSize = populationSize;
        this.mutationProbability = mutationProbability;
        this.environment = environment;
    }
}