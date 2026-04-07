using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Managers
{
    public class InstigatorsManager : Singleton<InstigatorsManager>
    {
        [SerializeField] private int m_IdCounterStartValue = 1;

        private readonly List<Instigator> m_Instigators = new();


        private int m_IdCounterNextValue;

        public override void Awake()
        {
            base.Awake();

            m_IdCounterNextValue = m_IdCounterStartValue;
        }

        public void AddInstigator(Instigator instigator)
        {
            if (m_Instigators.Contains(instigator)) return;
            m_Instigators.Add(instigator);
        }

        public void RemoveInstigator(Instigator instigator)
        {
            if (!m_Instigators.Contains(instigator)) return;
            m_Instigators.Remove(instigator);
        }

        public int GenerateId()
        {
            return m_IdCounterNextValue++;
        }
    }
}