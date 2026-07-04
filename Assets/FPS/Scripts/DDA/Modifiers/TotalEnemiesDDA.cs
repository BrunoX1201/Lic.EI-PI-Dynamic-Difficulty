namespace Unity.FPS.DDA
{
    public class TotalEnemiesDDA : DDAModifier<int>
    {
        private const int thresholdMin = 1;
        private const int thresholdMax = 2;

        public TotalEnemiesDDA(int value, int minValue = 1, int maxValue = 10)
            : base(value, minValue, maxValue, (a, b) => a + b, (a, b) => a * b)
        {
        }

        protected override DDADirection GetDifficultyDirection(int currentValue, int previousValue)
        {
            int difference = currentValue - previousValue;
            if (difference == 0) return DDADirection.Same;
            if (difference >= thresholdMax) return DDADirection.MuchHarder;
            if (difference >= thresholdMin) return DDADirection.Harder;
            if (difference >= -thresholdMin) return DDADirection.Easier;
            return DDADirection.MuchEasier;
        }
    }
}