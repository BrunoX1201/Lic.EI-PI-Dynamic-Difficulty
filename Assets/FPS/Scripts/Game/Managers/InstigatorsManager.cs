using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Telemetry;

namespace Unity.FPS.Game
{
    public class InstigatorsManager : Singleton<InstigatorsManager>
    {
        private readonly List<Instigator> m_instigators = new();
        private readonly int m_idCounterStartValue = Enum.GetValues(typeof(SpecialId)).Cast<int>().Max() + 1;
        private int m_idCounterNextValue;

        public InstigatorsManager()
        {
            m_idCounterNextValue = m_idCounterStartValue;
        }

        public void AddInstigator(Instigator instigator)
        {
            if (m_instigators.Contains(instigator))
            {
                return;
            }

            m_instigators.Add(instigator);
        }

        public void RemoveInstigator(Instigator instigator)
        {
            if (!m_instigators.Contains(instigator))
            {
                return;
            }

            m_instigators.Remove(instigator);
        }

        public int GenerateId()
        {
            return m_idCounterNextValue++;
        }
    }
}