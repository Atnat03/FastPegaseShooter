using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> musicList = new();
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private float volume = 1f;

    private void Start()
    {
        if (musicList.Count == 0 || audioSource == null)
            return;

        int randomIndex = Random.Range(0, musicList.Count);

        audioSource.clip = musicList[randomIndex];
        audioSource.volume = 0f;
        audioSource.Play();

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource.isPlaying)
            audioSource.volume = volume;
    }
}

/*
public class MusicManager : NetworkBusListener
{
    [SerializeField] private List<AudioClip> _musicList = new();
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _fadeDuration = 2f;

    private readonly SyncVar<int> _currentMusicIndex = new(-1);
    private readonly SyncVar<double> _musicStartTime = new(0);

    private float volume = 1f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _currentMusicIndex.OnChange += OnMusicIndexChanged;

        if (_currentMusicIndex.Value >= 0)
            SyncToCurrentMusic();

        ListenToEvent<OnPausePanelInit>(SendMusicManagerSignal);
    }

    void SendMusicManagerSignal(OnPausePanelInit data)
    {
        InvokeEvent(new OnMusicManagerLinkage { musicManager = this });
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _currentMusicIndex.OnChange -= OnMusicIndexChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(MusicLoop());
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            int next = PickRandom(_currentMusicIndex.Value);
            PlayMusicOnAll(next);

            yield return new WaitForSeconds(_musicList[next].length);
        }
    }

    private int PickRandom(int exclude)
    {
        if (_musicList.Count == 1)
            return 0;

        int index;
        do
        {
            index = Random.Range(0, _musicList.Count);
        }
        while (index == exclude);

        return index;
    }

    private void PlayMusicOnAll(int index)
    {
        _currentMusicIndex.Value = index;
        _musicStartTime.Value = Time.time;

        PlayMusicObserverRpc(index);
    }

    [ObserversRpc]
    private void PlayMusicObserverRpc(int index)
    {
        StartCoroutine(PlayWithFade(index));
    }

    private void OnMusicIndexChanged(int prev, int next, bool asServer)
    {
        if (!asServer)
            SyncToCurrentMusic();
    }

    private void SyncToCurrentMusic()
    {
        int index = _currentMusicIndex.Value;
        if (index < 0 || index >= _musicList.Count)
            return;

        AudioClip clip = _musicList[index];

        double elapsed = Time.time - _musicStartTime.Value;
        float startOffset = Mathf.Clamp((float)elapsed, 0f, clip.length - 0.1f);

        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.time = startOffset;
        _audioSource.volume = 0f;
        _audioSource.Play();

        StartCoroutine(FadeIn());
    }

    private IEnumerator PlayWithFade(int index)
    {
        _audioSource.Stop();
        _audioSource.clip = _musicList[index];
        _audioSource.volume = 0f;
        _audioSource.Play();

        yield return FadeIn();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, volume, elapsed / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = volume;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (_audioSource.isPlaying)
            _audioSource.volume = volume;
    }
}
*/