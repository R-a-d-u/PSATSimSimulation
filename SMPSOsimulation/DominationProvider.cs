using static SMPSOsimulation.SMPSOOrchestrator;

namespace SMPSOsimulation;

public interface DominationProvider
{
    bool IsDominated(double[] resultToCheck, List<double[]> resultsToCheckAgainst);

    bool IsDominated(Particle particleToCheck, List<Particle> particlesToCheckAgainst)
    {
        return IsDominated(particleToCheck.positionWithResult.result,
            particlesToCheckAgainst.Select(particle => particle.positionWithResult.result).ToList());
    }
}