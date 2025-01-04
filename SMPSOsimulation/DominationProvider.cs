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

    bool IsDominated(PositionWithResult leader, List<Particle> swarm)
    {
        return IsDominated(leader.result, swarm.Select(particle => particle.positionWithResult.result).ToList());
    }

    bool IsDominated(Particle particle, List<PositionWithResult> leadersArchive)
    {
        return IsDominated(particle.positionWithResult.result, leadersArchive.Select(leader => leader.result).ToList());
    }

    bool IsDominated(PositionWithResult leader, List<PositionWithResult> leadersArchive)
    {
        return IsDominated(leader.result, leadersArchive.Select(leader => leader.result).ToList());
    }
}