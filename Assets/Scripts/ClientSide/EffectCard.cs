using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpsDojo
{
    [CreateAssetMenu(fileName = "New Effect Card", menuName = "Effect Card")]
    public class EffectCard: Card
    {
        public EffectType effectType;
        public int staminaCost;
    }

    public enum EffectType
    {
        HealthUp,
        DamageUp,
        Guard,
    }
}
