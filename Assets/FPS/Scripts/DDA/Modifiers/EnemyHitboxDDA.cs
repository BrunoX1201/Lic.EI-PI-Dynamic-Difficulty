namespace Unity.FPS.DDA
{
    public class EnemyHitboxDDA : DDAModifier<float>
    {
        private const float thresholdMin = 0.05f;
        private const float thresholdMax = 0.1f;

        public EnemyHitboxDDA(float value, float minValue = 0.25f, float maxValue = 2f)
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