using System;
using UnityEngine;

namespace Dev.Scripts.Characters
{
    public struct Events
    {
        public static Action PlayerDetect;

        public static void DestroyEvents()
        {
            PlayerDetect = null;
        }
    }
    public class Character : MonoBehaviour
    {
          public string characterName;
          public int cost;


          public Animator animator;
          public Sprite icon;
          public Sprite CharacterBg;

          [Header("Sound")]
          public AudioClip jumpSound;
          public AudioClip hitSound;
          public AudioClip deathSound;
          
    }
}