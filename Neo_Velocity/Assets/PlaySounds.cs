using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class PlaySounds : MonoBehaviour {
    public AudioSource jump;
    public AudioSource shoot;
    public AudioSource slide;
    // Start is called before the first frame update
    public  void playJump() {
        jump.Play();
    }

    public  void playSlide() {
        slide.Play();
    }

    public  void playShoot() {
        shoot.Play();
    }
}
