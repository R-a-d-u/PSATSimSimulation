namespace SMPSOsimulation;

public class SMPSODomination : DominationProvider
{
    public bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst)
    {
        foreach (double[] currentResult in resultsToCheckAgainst)
        {
            if ((currentResult[0] <= resultToCheck[0] && currentResult[1] <= resultToCheck[1]) &&
                (currentResult[0] < resultToCheck[0] || currentResult[1] < resultToCheck[1]))
                return true;
        }

        return false;
    }
}