using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.Characters
{
    using UnityEngine;

    public class Character : MonoBehaviour
    {
        [Header("Character Information")]
        public string characterName;
        public Sprite icon;
        public Sprite characterBg;

        [Header("Animation and Sounds")]
        public Animator animator;
        public AudioClip jumpSound;
        public AudioClip hitSound;
        public AudioClip deathSound;
        
    }

}