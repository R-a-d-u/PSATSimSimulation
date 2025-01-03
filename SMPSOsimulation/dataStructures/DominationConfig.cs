namespace SMPSOsimulation.dataStructures
{
    public class DominationConfig
    {
        public enum DominationType
        {
            SMPSO,
            WEIGHT,
            LEXICOGRAPHIC
        }

        public enum PrefferedObjective
        {
            CPI,
            ENERGY
        }

        public DominationType dominationType;
        public double? wCPI, wEnergy;
        public PrefferedObjective? prefferedObjective;
        public double? tolerance;

        public bool IsWithinConstraints()
        {
            if (wCPI is not null && wCPI <= 0) return false;
            if (wEnergy is not null && wEnergy <= 0) return false;
            if (tolerance is not null && tolerance <= 0) return false;
            return true;
        }

        private DominationConfig(DominationType dominationType, double? wCPI, double? wEnergy, PrefferedObjective? prefferedObjective, double? tolerance)
        {
            this.dominationType = dominationType;
            this.wCPI = wCPI;
            this.wEnergy = wEnergy;
            this.prefferedObjective = prefferedObjective;
            this.tolerance = tolerance;

            if (!this.IsWithinConstraints())
                throw new Exception("Domination config created with illegal params");
        }

        public static DominationConfig GetSMPSODominationConfig()
        {
            return new DominationConfig(DominationType.SMPSO, null, null, null, null);
        }

        public static DominationConfig GetWeightedSumDominationConfig(double wCPI, double wEnergy)
        {
            return new DominationConfig(DominationType.WEIGHT, wCPI, wEnergy, null, null);
        }

        public static DominationConfig GetLexicographicDominationConfig(PrefferedObjective prefferedObjective, double tolerance)
        {
            return new DominationConfig(DominationType.LEXICOGRAPHIC, null, null, prefferedObjective, tolerance);
        }

    }
}
