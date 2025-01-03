using static SMPSOsimulation.dataStructures.DominationConfig;

namespace SMPSOsimulation;

public class LexicographicDomination : DominationProvider
{
    private PrefferedObjective preferredObjective;
    private double tolerance;

    public LexicographicDomination(PrefferedObjective preferredObjective, double tolerance)
    {
        this.preferredObjective = preferredObjective;
        this.tolerance = tolerance;
    }

    public bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst)
    {
        foreach (var currentResult in resultsToCheckAgainst)
        {
            var prefferedObjectiveIndex = (int)preferredObjective;

            if (prefferedObjectiveIndex != 0 && prefferedObjectiveIndex != 1)
                throw new Exception("You need to rethink IsDominated in lexicographic");

            var nonPrefferedObjective = 1 - prefferedObjectiveIndex;

            if (resultToCheck[prefferedObjectiveIndex].Equals(currentResult[prefferedObjectiveIndex]))
            {
                if (resultToCheck[nonPrefferedObjective] > currentResult[nonPrefferedObjective])
                    return true;
            }
            else if (resultToCheck[prefferedObjectiveIndex] > currentResult[prefferedObjectiveIndex])
                return true;
        }

        return false;
    }
}