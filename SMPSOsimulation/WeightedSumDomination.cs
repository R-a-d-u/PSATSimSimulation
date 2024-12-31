namespace SMPSOsimulation;

public class WeightedSumDomination : DominationProvider
{
    private double wCPI, wEnergy;

    public WeightedSumDomination(double wCpi, double wEnergy)
    {
        wCPI = wCpi;
        this.wEnergy = wEnergy;
    }

    public bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst)
    {
        double sumToCheck = wCPI * resultToCheck[0] + wEnergy * resultToCheck[1];
        foreach (double[] resultToCheckAgainst in resultsToCheckAgainst)
        {
             double currentSum = wCPI * resultToCheckAgainst[0] + wEnergy * resultToCheckAgainst[1];
             if (sumToCheck > currentSum)
                 return true;
        }

        return false;
    }
}