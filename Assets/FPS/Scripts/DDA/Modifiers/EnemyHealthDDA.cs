namespace Unity.FPS.DDA
{
    public class EnemyHealthDDA : DDAModifier<float>
    {
        private const float thresholdMin = 0.2f;
        private const float thresholdMax = 0.4f;

        public EnemyHealthDDA(float value, float minValue = 0.1f, float maxValue = 3f)
            : base(value, minValue, maxValue, (a, b) => a + b, (a, b) => a * b)
        {
        }

        protected override DDADirection GetDifficultyDirection(float currentValue, float previousValue)
        {
            float difference = currentValue - previousValue;
            if (difference == 0) return DDADirection.Same;
            if (difference >= thresholdMax) return DDADirection.MuchHarder;
            if (difference >= thresholdMin) return DDADirection.Harder;
            if (difference >= -thresholdMin) return DDADirection.Easier;
            return DDADirection.MuchEasier;
        }
    }
}