namespace Unity.FPS.DDA
{
    public class TotalEnemiesDDA : DDAModifier<int>
    {
        private const int k_thresholdMin = 1;
        private const int k_thresholdMax = 2;

        public TotalEnemiesDDA(int value, int minValue = 1, int maxValue = 10)
            : base(value, minValue, maxValue, (a, b) => a + b, (a, b) => a * b)
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