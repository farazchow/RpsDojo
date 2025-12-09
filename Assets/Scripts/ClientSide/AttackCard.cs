using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpsDojo
{
    [CreateAssetMenu(fileName = "New Attack Card", menuName = "Attack Card")]
    public class AttackCard: Card
    {
        public AttackType attackType;
        public int damage;
    }

    public enum AttackType
    {
        Rock,
        Paper,
        Scissors
    }
}