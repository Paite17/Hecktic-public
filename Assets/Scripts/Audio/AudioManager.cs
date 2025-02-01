using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;

    public AudioMixer mixer;

    private Scene currentScene;

    private string sceneName;
    void Awake()
    {
        AudioMixerGroup[] audioMixGroup = mixer.FindMatchingGroups("Master");
        if (instance == null)
        {
            instance = this;
            currentScene = SceneManager.GetActiveScene();
            sceneName = currentScene.name;
        }
        else
        {
            Destroy(gameObject);

            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = audioMixGroup[0];

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        /*if (sceneName == "MainScene")
        {
            Play("Dungeon Music 1");
            StopMusic("Battle Theme 1");
        } */
    }

    // play track specified 
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            //Debug.LogWarning(name + " doesn't exist!");
            return;
        }
        Debug.Log(s.name);
        s.source.Play();
    }

    // stop track specified
    public void StopMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " doesn't exist!");
            return;
        }
        s.source.Stop();

    }

    // stops everything
    public void StopAllMusic()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].source.Stop();
        }
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning(name + " doesn't exist!");
            return false;
        }

        return s.source.isPlaying;
    }

    public void ChangeVolume(string name, float amount)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            //Debug.LogWarning(name + " doesn't exist!");
            return;
        }
        s.source.volume = amount;
    }
}
