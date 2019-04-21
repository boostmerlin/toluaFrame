using System;
using System.Collections.Generic;
using UnityEngine;

using LuaFramework;

/// <summary>
/// 游戏音效管理组件
/// </summary>
public class SoundManager : Manager
{
    /// <summary>
    /// 控制游戏全局音量
    /// </summary>
    public float SoundVolume
    {
        get
        {
            return _soundVolume;
        }
        set
        {
            _soundVolume = Mathf.Clamp(value, 0, 1);
            foreach (SoundData clip in m_clips[SoundType.Sound].Values)
            {
                clip.Volume = _soundVolume * clip.volume;
            }
        }
    }
    private float _soundVolume = 1.0f;

    public bool SingleMusicOnly = true;

    //所有音效
    private Dictionary<SoundType, Dictionary<string, SoundData>> m_clips = new Dictionary<SoundType, Dictionary<string, SoundData>>()
    { {SoundType.Music, new Dictionary<string, SoundData>() },
    {SoundType.Sound, new Dictionary<string, SoundData>()} };

    //根物体
    private Transform root;

    /// <summary>
    /// 音乐静音
    /// </summary>
    public bool MusicMute
    {
        get { return _musicMute; }
        set
        {
            _musicMute = value;
            foreach (var soundData in m_clips[SoundType.Music].Values)
            {
                soundData.Mute = _musicMute;
            }
            PlayerPrefs.SetInt("MusicMute", value ? 1 : 0);
        }
    }
    private bool _musicMute = false;

    /// <summary>
    /// 音效静音
    /// </summary>
    public bool SoundMute
    {
        get { return _soundMute; }
        set
        {
            _soundMute = value;
            foreach (var soundData in m_clips[SoundType.Sound].Values)
            {
                soundData.Mute = _soundMute;
            }
            PlayerPrefs.SetInt("SoundMute", value ? 1 : 0);
        }
    }
    private bool _soundMute = false;

    public void Awake()
    {
        _musicMute = PlayerPrefs.GetInt("MusicMute", 0) == 1;
        _soundMute = PlayerPrefs.GetInt("SoundMute", 0) == 1;
        root = transform;
    }

    private SoundData GetAudioSource(string clipName)
    {
        if (m_clips[SoundType.Music].ContainsKey(clipName))
            return m_clips[SoundType.Music][clipName];
        if (m_clips[SoundType.Sound].ContainsKey(clipName))
            return m_clips[SoundType.Sound][clipName];
        return null;
    }

    private void AddClip(string clipName, SoundData data)
    {
        data.IsPause = false;
        data.transform.SetParent(root);
        SoundData sd = GetAudioSource(clipName);
        if (sd != null)
        {
            Debug.LogError("Repeat sound data: " + clipName);
        }
        else
        {
            m_clips[data.soundType].Add(clipName, data);
        }
    }

    public void Play(string clipName,  float volume = -1, float delay = -1, bool forceReplay = false)
    {
        SoundData sd = GetAudioSource(clipName);
        if (sd)
        {
            if (sd.soundType == SoundType.Music)
            {
                sd.Mute = _musicMute;
                if (SingleMusicOnly)
                {
                    foreach (var soundData in m_clips[SoundType.Music].Values)
                    {
                        if (soundData != sd)
                            soundData.Dispose();
                    }
                    m_clips[SoundType.Music].Clear();
                    AddClip(clipName, sd);
                }
            }
            else
            {
                sd.Mute = _soundMute;
            }
            sd.isForceReplay = forceReplay;
            if (delay > 0)
            {
                sd.delay = delay;
            }
            if (volume > 0)
            {
                sd.volume = volume;
            }
            _playSound(clipName, sd);
            return;
        }
        LoadSoundAsnyc(clipName, (objs) =>
        {
            if (objs.Length == 0)
                return;
            sd = GetAudioSource(clipName);
            if (sd == null)
            {
                var go = Instantiate(objs[0]) as GameObject;
                sd = go.GetComponent<SoundData>();
                AddClip(clipName, sd);
            }

            if (sd.soundType == SoundType.Music)
            {
                sd.isForceReplay = forceReplay;
                if (SingleMusicOnly)
                {
                    foreach (var soundData in m_clips[SoundType.Music].Values)
                    {
                        if(soundData != sd)
                            soundData.Dispose();
                    }
                    m_clips[SoundType.Music].Clear();
                    AddClip(clipName, sd);
                }
            }
            if(delay > 0)
            {
                sd.delay = delay;
            }
            if(volume > 0)
            {
                sd.volume = volume;
            }
            _playSound(clipName, sd);
        });
    }

    private void LoadSoundAsnyc(string soundName, Action<UnityEngine.Object[]> action)
    {
        ResourceManager resourceManager = LuaHelper.GetResManager();
        resourceManager.LoadAsset<GameObject>(soundName, new string[] { System.IO.Path.GetFileNameWithoutExtension(soundName) }, action);
    }

    //播放SoundData
    private void _playSound(string clipName, SoundData asource)
    {
        if (null == asource)
            return;
        bool forceReplay = asource.isForceReplay;
        asource.audio.volume = Mathf.Clamp(asource.volume, 0, 1) * SoundVolume;
        asource.audio.loop = asource.isLoop;
        if (!forceReplay)
        {
            if (!asource.IsPlaying)
            {
                if (!asource.IsPause)
                    if (asource.delay > 0)
                    {
                        asource.audio.PlayDelayed(asource.delay);
                    }
                    else
                    {
                        asource.audio.Play();
                    }
                else
                {
                    Resume(clipName);
                }
            }
        }
        else
        {
            asource.audio.PlayDelayed(asource.delay);
            asource.audio.PlayScheduled(0);
        }
    }

    /// <summary>
    /// 停止并销毁声音
    /// </summary>
    /// <param name="clipName"></param>
    public void Stop(string clipName)
    {
        SoundData data = GetAudioSource(clipName);
        if (null != data)
        {
            data.Dispose();
        }
    }

    /// <summary>
    /// 暂停声音
    /// </summary>
    /// <param name="clipName"></param>
    public void Pause(string clipName)
    {
        SoundData data = GetAudioSource(clipName);
        if (null != data)
        {
            data.IsPause = true;
            data.audio.Pause();
        }
    }

    /// <summary>
    /// 继续播放
    /// </summary>
    /// <param name="clipName"></param>
    public void Resume(string clipName)
    {
        SoundData data = GetAudioSource(clipName);
        if (null != data)
        {
            data.IsPause = false;
            data.audio.UnPause();
        }
    }

    /// <summary>
    /// 销毁所有声音
    /// </summary>
    public void DisposeAll()
    {
        foreach (var items in m_clips)
        {
            foreach(var item in items.Value)
                item.Value.Dispose();
        }
        m_clips.Clear();
    }
}