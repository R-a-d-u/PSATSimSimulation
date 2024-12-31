namespace SMPSOsimulation;

public interface DominationProvider
{
    bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst);
}