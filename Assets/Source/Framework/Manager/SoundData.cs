using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundData : MonoBehaviour
{
    public new AudioSource audio;

    /// <summary>
    /// 是否强制重新播放
    /// </summary>
    public bool isForceReplay = false;

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public bool isLoop = false;

    /// <summary>
    /// 音量
    /// </summary>
    public float volume = 1;

    /// <summary>
    /// 延迟
    /// </summary>
    public float delay = 0;

    public AudioSource GetAudio()
    {
        return audio;
    }

    public bool IsPlaying
    {
        get
        {
            return audio != null && audio.isPlaying;
        }
    }
    public bool IsPause
    {
        get;
        set;
    }
    public void Dispose()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 音效类型
    /// </summary>
    public SoundType soundType;

    public bool Mute
    {
        get { return audio.mute; }
        set { audio.mute = value; }
    }

    public float Volume
    {
        get { return audio.volume; }
        set { audio.volume = value; }
    }
}

/// <summary>
/// 音效类型
/// </summary>
public enum SoundType
{
    Sound,//短音乐
    Music,//长音乐
}
