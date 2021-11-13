using MyBox;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // Audio players components.
    //public AudioSource effectsSource1, effectsSource2, effectsSource3, effectsSource4, effectsSource5, effectsSource6, effectsSource7, effectsSource8, effectsSource9, effectsSource10, effectsSource11, effectsSource12, effectsSource13, effectsSource14, effectsSource15, effectsSource16, effectsSource17, effectsSource18;
    public float GLOBAL_SFX_VOLUME = 1f;
    public float MAX_VOL_MUSIC = 0.65f;
    public AudioSource[] constantsSounds;
    public enum ConstantSoundsEnum { Music, LastSeconds, Vision, }
    private Queue<AudioSource> soundPool;
    public int soundPoolCount;
    public AudioSource audioPrefab;

    public AudioClip[] drawCardSounds;
    public AudioClip[] burnCardSounds;
    public AudioClip[] windCardSounds;
    public AudioClip[] shuffleCardsSounds;
    public AudioClip startSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip betAsk;
    public AudioClip betYes;
    public AudioClip btnClick1;
    public AudioClip btnClick2;
    public AudioClip btnClick3;
    public AudioClip btnCantClick;
    public AudioClip btnNo1;
    public AudioClip btnNo2;
    public AudioClip hpHit;
    public AudioClip hpRise;
    public AudioClip slash1;
    public AudioClip slash2;
    public AudioClip puUse;
    public AudioClip puFreeze;
    public AudioClip puSee;
    public AudioClip dissolve;
    public AudioClip endTurnGong;
    public AudioClip openDrawer;
    public AudioClip closeDrawer;
    public AudioClip iceProjectile;
    public AudioClip fireProjectile;
    public AudioClip timerLastSeconds;
    public AudioClip visionDrone;
    public AudioClip musicSound;
    public AudioClip endRoundGong;
    public AudioClip coinHit;



    // Random pitch adjustment range.
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    private bool musicOn;

    private void Awake()
    {
        GLOBAL_SFX_VOLUME = Values.Instance.sfxVolume;
        MAX_VOL_MUSIC = Values.Instance.musicVolume;
        InitPool();

    }

    public void PlayMusic()
    {
        if (musicOn)
        {
            PlayConstantSound(ConstantSoundsEnum.Music, false);
        }
        else
        {
            PlayConstantSound(ConstantSoundsEnum.Music, true);
        }
        musicOn = !musicOn;
    }
    /*private void Start()
    {
        PlayConstantSound(ConstantSoundsEnum.Music, true);
        musicOn = true;
    }*/


    public void StopMusic()
    {
        PlayConstantSound(ConstantSoundsEnum.Music, false);
        musicOn = false;
    }
    private void InitPool()
    {
        soundPool = new Queue<AudioSource>();
        for (int i = 0; i < soundPoolCount; i++)
        {
            AudioSource obj = Instantiate(audioPrefab, gameObject.transform);
            obj.name = "AS:" + i;
            soundPool.Enqueue(obj);
        }
    }


    // Play a single clip through the sound effects source.
    public async Task PlayAsync(AudioClip clip)
    {
        AudioSource currentSource = await GetAvailableAudioSource();
        currentSource.clip = clip;
        currentSource.volume = GLOBAL_SFX_VOLUME;
        currentSource.Play();
        soundPool.Enqueue(currentSource);
    }
    /* public async Task StopSound(AudioClip clip)
     {
         AudioSource currentSource = await GetPlayingSound();

         currentSource.clip = clip;
         currentSource.Stop();
     }*/

    /* private AudioSource GetPlayingSound()
     {
         if (soundPool.Count > 0)
         {
             for (int i = 0; i < soundPool.Count; i++)
             {
                 AudioSource currentSource = soundPool.Dequeue();

                 if (currentSource.isPlaying)
                 {
                     return currentSource;
                 }
             }
         }
         return null;
     }*/
    private async Task<AudioSource> GetAvailableAudioSource()
    {
        if (soundPool.Count > 0)
        {
            for (int i = 0; i < soundPool.Count; i++)
            {
                AudioSource currentSource = soundPool.Dequeue();
                if (!currentSource.isPlaying)
                {
                    return currentSource;
                }
                else
                {
                    Debug.LogError("oh no sound");
                }
                await Task.Yield();

            }
        }
        return null;
    }

    // Play a single clip through the music source.
    public void PlayConstantSound(ConstantSoundsEnum sound, bool enable)
    {
        int audioSourceIndex = sound.GetHashCode();
        constantsSounds[audioSourceIndex].clip = GetConstantSoundClip(sound);
        if (enable)
        {
            constantsSounds[audioSourceIndex].Play();
            StartCoroutine(FadeIn(constantsSounds[audioSourceIndex], 1f));
            if (sound == ConstantSoundsEnum.Music)
            {

                constantsSounds[audioSourceIndex].loop = true;
            }
        }
        else
        {

            constantsSounds[audioSourceIndex].Stop();
        }

    }

    public  IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.volume = 0f;
        audioSource.Play();
        while (audioSource.volume < MAX_VOL_MUSIC)
        {
            audioSource.volume +=  Time.deltaTime/2 ;
            if (audioSource.volume >= MAX_VOL_MUSIC)
            {
                audioSource.volume = MAX_VOL_MUSIC;
                break;
            }
            yield return null;
        }
    }


    private void OnDestroy()
    {
        StopMusic();
    }
    private AudioClip GetConstantSoundClip(ConstantSoundsEnum sound)
    {
        switch (sound)
        {
            case ConstantSoundsEnum.Music:
                {
                    return musicSound;

                }
            case ConstantSoundsEnum.LastSeconds:
                {
                    return timerLastSeconds;

                }
            case ConstantSoundsEnum.Vision:
                {
                    return visionDrone;
                }
        }
        return null;
    }

    // Play a random clip from an array, and randomize the pitch slightly.

    public void RandomSoundEffect(SoundName whatSounds)
    {
        Task task = RandomSoundEffectTask(whatSounds);
        task.Wait();
    }
    private async Task RandomSoundEffectTask(SoundName whatSounds)
    {

        AudioClip[] targetClipSound = null;
        switch (whatSounds)
        {
            case SoundName.DrawCard:
                {
                    targetClipSound = drawCardSounds;
                    break;
                }
            case SoundName.BurnCard:
                {
                    targetClipSound = burnCardSounds;
                    break;
                }
            case SoundName.ShuffleDeck:
                {
                    targetClipSound = shuffleCardsSounds;
                    break;
                }
            case SoundName.WindSound:
                {
                    targetClipSound = windCardSounds;
                    break;
                }
        }

        int randomIndex = Random.Range(0, targetClipSound.Length);
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);

        //EffectsSource.pitch = randomPitch;
        await PlayAsync(targetClipSound[randomIndex]);


    }

    public void PlaySingleSound(SoundName whatSound)
    {
        Task task = PlaySingleSoundTask(whatSound);
        task.Wait();
    }
    private async Task PlaySingleSoundTask(SoundName whatSound)
    {
        AudioClip soundToPlay = null;
        switch (whatSound)
        {
            case SoundName.StartRound:
                {
                    soundToPlay = startSound;
                    break;
                }
            case SoundName.Win:
                {
                    soundToPlay = winSound;
                    break;
                }
            case SoundName.Lose:
                {
                    soundToPlay = loseSound;
                    break;
                }
            case SoundName.HpHit:
                {
                    soundToPlay = hpHit;
                    break;
                }
            case SoundName.HpRise:
                {
                    soundToPlay = hpRise;
                    break;
                }
            case SoundName.Slash1:
                {
                    soundToPlay = slash1;
                    break;
                }
            case SoundName.Slash2:
                {
                    soundToPlay = slash2;
                    break;
                }
            case SoundName.PuUse:
                {
                    soundToPlay = puUse;
                    break;
                }
            case SoundName.PuFreeze:
                {
                    soundToPlay = puFreeze;
                    break;
                }
            case SoundName.PuSee:
                {
                    soundToPlay = puSee;
                    break;
                }
            case SoundName.BtnClick1:
                {
                    soundToPlay = btnClick1;
                    break;
                }
            case SoundName.BtnClick2:
                {
                    soundToPlay = btnClick2;
                    break;
                }
            case SoundName.BtnClick3:
                {
                    soundToPlay = btnClick3;
                    break;
                }
            case SoundName.BtnClickNo1:
                {
                    soundToPlay = btnNo1;
                    break;
                }
            case SoundName.BtnClickNo2:
                {
                    soundToPlay = btnNo2;
                    break;
                }
            case SoundName.CantClick:
                {
                    soundToPlay = btnCantClick;
                    break;
                }
            case SoundName.BetAsk:
                {
                    soundToPlay = betAsk;
                    break;
                }
            case SoundName.BetYes:
                {
                    soundToPlay = betYes;
                    break;
                }
            case SoundName.Dissolve:
                {
                    soundToPlay = dissolve;
                    break;
                }
            case SoundName.EndTurnGong:
                {
                    soundToPlay = endTurnGong;
                    break;
                }
            case SoundName.TimerLastSeconds:
                {
                    soundToPlay = timerLastSeconds;
                    break;
                }
            case SoundName.OpenDrawer:
                {
                    soundToPlay = openDrawer;
                    break;
                }
            case SoundName.CloseDrawer:
                {
                    soundToPlay = closeDrawer;
                    break;
                }
            case SoundName.IceProjectile:
                {
                    soundToPlay = iceProjectile;
                    break;
                }
            case SoundName.FireProjectile:
                {
                    soundToPlay = fireProjectile;
                    break;
                }
            case SoundName.VisionDrone:
                {
                    soundToPlay = visionDrone;
                    break;
                }
            case SoundName.EndRoundGong:
                {
                    soundToPlay = endRoundGong;
                    break;
                }
            case SoundName.CoinHit:
                {
                    soundToPlay = coinHit;
                    break;
                }
        }

        await PlayAsync(soundToPlay);

    }

    public enum SoundName
    {
        DrawCard,
        BurnCard,
        ShuffleDeck,
        StartRound,
        Win,
        Lose,
        HpHit,
        HpRise,
        Slash1,
        Slash2,
        PuUse,
        PuArmagedon,
        PuFreeze,
        PuHeal,
        PuSee,
        BtnClick1,
        BtnClick2,
        BtnClick3,
        BtnClickNo1,
        BtnClickNo2,
        CantClick,
        BetAsk,
        BetYes,
        WindSound,
        Dissolve,
        EndTurnGong,
        TimerLastSeconds,
        OpenDrawer,
        CloseDrawer,
        IceProjectile,
        FireProjectile,
        VisionDrone,
        EndRoundGong,
        CoinHit,
    }
}
