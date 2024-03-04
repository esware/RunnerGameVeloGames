using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.Characters
{
    public class Character : MonoBehaviour
    {
          public string characterName;
          public int cost;


          public Animator animator;
          public Sprite icon;
          public Sprite characterBg;

          [Header("Sound")]
          public AudioClip jumpSound;
          public AudioClip hitSound;
          public AudioClip deathSound;
          
    }
}