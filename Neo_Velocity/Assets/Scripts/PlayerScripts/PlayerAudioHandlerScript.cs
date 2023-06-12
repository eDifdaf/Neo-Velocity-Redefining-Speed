using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioHandlerScript : MonoBehaviour
{
    [SerializeField] AudioClip Jump;
    [SerializeField] AudioClip Shoot;
    [SerializeField] AudioClip Slide;
    public void PlayAudioJump(Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(Jump, position, volume);
    }
    public void PlayAudioShoot(Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(Shoot, position, volume);
    }
    public void PlayAudioSlide(Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(Slide, position, volume);
    }
}
