using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Managers
{
    public class InstigatorsManager : Singleton<InstigatorsManager>
    {
        [SerializeField] private int m_idCounterStartValue = 1;

        private readonly List<Instigator> m_instigators = new();
        private int m_idCounterNextValue;

        public override void Awake()
        {
            base.Awake();

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