namespace Unity.FPS.DDA
{
    public class MobileHealth : DDAModifier<float>
    {
        private const float k_thresholdMin = 0.2f;
        private const float k_thresholdMax = 0.4f;

        public MobileHealth(float value)
            : base(value, (a, b) => a + b)
        {
        }

        protected override DDADirection GetDifficultyDirection(float currentValue, float previousValue)
        {
            float difference = currentValue - previousValue;
            if (difference == 0) return DDADirection.Same;
            if (difference >= k_thresholdMax) return DDADirection.MuchHarder;
            if (difference >= k_thresholdMin) return DDADirection.Harder;
            if (difference >= -k_thresholdMin) return DDADirection.Easier;
            return DDADirection.MuchEasier;
        }
    }
}