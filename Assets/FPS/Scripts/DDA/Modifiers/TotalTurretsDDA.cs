namespace Unity.FPS.DDA
{
    public class TotalTurretsDDA : DDAModifier<int>
    {
        private const int k_thresholdMin = 1;
        private const int k_thresholdMax = 2;

        public TotalTurretsDDA(int value) : base(value, (a, b) => a + b)
        {
        }

        protected override DDADirection GetDifficultyDirection(int currentValue, int previousValue)
        {
            int difference = currentValue - previousValue;
            if (difference == 0) return DDADirection.Same;
            if (difference >= k_thresholdMax) return DDADirection.MuchHarder;
            if (difference >= k_thresholdMin) return DDADirection.Harder;
            if (difference >= -k_thresholdMin) return DDADirection.Easier;
            return DDADirection.MuchEasier;
        }
    }
}