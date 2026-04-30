using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class MusicManager : NetworkBehaviour
{

    [SerializeField] private List<AudioClip> _musicList = new();
    [SerializeField] private AudioSource _sourceA;
    [SerializeField] private AudioSource _sourceB;
    [SerializeField] private float _fadeDuration = 2f;

    private readonly SyncVar<int> _currentMusicIndex = new SyncVar<int>(-1);
    private readonly SyncVar<double> _musicStartTime = new SyncVar<double>(0);

    private AudioSource _activeSource;
    private AudioSource _inactiveSource;
    private bool _isTransitioning = false;

    private void Awake()
    {
        _activeSource = _sourceA;
        _inactiveSource = _sourceB;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _currentMusicIndex.OnChange += OnMusicIndexChanged;

        if (_currentMusicIndex.Value >= 0)
            SyncToCurrentMusic();
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

            float clipLength = _musicList[next].length;
            yield return new WaitForSeconds(clipLength - _fadeDuration);
        }
    }

    private int PickRandom(int exclude)
    {
        if (_musicList.Count == 1) return 0;

        int index;
        do { index = Random.Range(0, _musicList.Count); }
        while (index == exclude);
        return index;
    }

    private void PlayMusicOnAll(int index)
    {
        _currentMusicIndex.Value = index;
        _musicStartTime.Value = (double)Time.time;
        PlayMusicObserverRpc(index);
    }
    
    [ObserversRpc]
    private void PlayMusicObserverRpc(int index)
    {
        StartCoroutine(CrossFadeTo(index));
    }
    
    private void OnMusicIndexChanged(int prev, int next, bool asServer)
    {
        if (!asServer)
            SyncToCurrentMusic();
    }

    private void SyncToCurrentMusic()
    {
        int index = _currentMusicIndex.Value;
        if (index < 0 || index >= _musicList.Count) return;

        AudioClip clip = _musicList[index];

        double elapsed = Time.time - _musicStartTime.Value;
        float startOffset = Mathf.Clamp((float)elapsed, 0f, clip.length - 0.1f);

        _activeSource.clip = clip;
        _activeSource.time = startOffset;
        _activeSource.volume = 1f;
        _activeSource.Play();
    }


    private IEnumerator CrossFadeTo(int index)
    {
        if (_isTransitioning) yield break;
        _isTransitioning = true;

        AudioClip nextClip = _musicList[index];

        _inactiveSource.clip = nextClip;
        _inactiveSource.volume = 0f;
        _inactiveSource.Play();

        float elapsed = 0f;
        float startVolume = _activeSource.volume;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _fadeDuration;

            _activeSource.volume = Mathf.Lerp(startVolume, 0f, t);
            _inactiveSource.volume = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        _activeSource.Stop();
        _activeSource.volume = 0f;

        (_activeSource, _inactiveSource) = (_inactiveSource, _activeSource);

        _isTransitioning = false;
    }
}