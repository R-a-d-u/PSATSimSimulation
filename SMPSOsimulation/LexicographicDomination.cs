namespace SMPSOsimulation;

public class LexicographicDomination : DominationProvider
{
    private int preferredObjective;
    private double tolerance;

    public LexicographicDomination(int preferredObjective, double tolerance)
    {
        this.preferredObjective = preferredObjective;
        this.tolerance = tolerance;
    }

    public bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst)
    {
        foreach (var currentResult in resultsToCheckAgainst)
        {
            var nonPrefferedObjective = 1 - preferredObjective;

            if (Math.Abs(resultToCheck[preferredObjective] - currentResult[preferredObjective]) < tolerance)
            {
                if (resultToCheck[nonPrefferedObjective] > currentResult[nonPrefferedObjective])
                    return true;
            }
            else if (resultToCheck[preferredObjective] > currentResult[preferredObjective])
                return true;
        }

        return false;
    }
}