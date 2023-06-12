using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    AudioSource audioPlayer;

    void Awake()
    {
        audioPlayer = gameObject.GetComponent<AudioSource>();
        // Get rid of any old Players, unless they use the same Music
        GameObject.FindGameObjectsWithTag("Music").Where(o => o != gameObject).ToList().ForEach(o => {
            if (o.GetComponent<AudioSource>().clip == audioPlayer.clip)
            {
                Destroy(gameObject);
                return;
            }
            Destroy(o);
            });
        DontDestroyOnLoad(gameObject);
    }
}
