using UnityEngine;
using Rewired;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class MusicSwitcher : MonoBehaviour
{
    [Header("Rewired")]
    public int playerId = 0;
    private Player player;

    [Header("Music")]
    public List<AudioSource> tracks = new List<AudioSource>();
    public int currentTrackIndex = 0;
    public bool randomStart = true;

    [Header("Crossfade Settings")]
    [Range(0f, 5f)] public float crossfadeDuration = 0.5f;

    [Header("Volume")]
    [Range(0f, 1f)] public float volumeStep = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI trackNameText;
    public float displayTime = 3f;
    public float fadeSpeed = 4f;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerId);

        foreach (var t in tracks)
        {
            if (t != null)
                t.Stop();
        }

        if (tracks.Count > 0)
        {
            if (randomStart)
                currentTrackIndex = Random.Range(0, tracks.Count);

            PlayCurrentTrack();
        }
    }

    private void Update()
    {
        if (tracks.Count == 0) return;

        if (player.GetButtonDown("Switch Track"))
            SwitchTrack();

        if (player.GetButtonDown("Raise Volume"))
            AdjustVolume(volumeStep);

        if (player.GetButtonDown("Lower Volume"))
            AdjustVolume(-volumeStep);

        AudioSource current = tracks[currentTrackIndex];
        if (current != null && !current.isPlaying && current.time == 0f)
        {
            SwitchTrack();
        }
    }

    public void SwitchTrack()
    {
        if (tracks.Count == 0 || tracks[currentTrackIndex] == null)
            return;

        StartCoroutine(CrossfadeTracks());
    }

    private IEnumerator CrossfadeTracks()
    {
        AudioSource current = tracks[currentTrackIndex];
        currentTrackIndex = (currentTrackIndex + 1) % tracks.Count;
        AudioSource next = tracks[currentTrackIndex];
        if (next == null) yield break;

        next.volume = 0f;
        next.Play();

        float time = 0f;
        while (time < crossfadeDuration)
        {
            float t = time / crossfadeDuration;
            if (current != null)
                current.volume = Mathf.Lerp(1f, 0f, t);
            next.volume = Mathf.Lerp(0f, 1f, t);
            time += Time.deltaTime;
            yield return null;
        }

        if (current != null)
        {
            current.Stop();
            current.volume = 1f;
        }

        next.volume = 1f;
        ShowTrackName(next.clip != null ? next.clip.name : $"Track {currentTrackIndex + 1}");
    }

    public void PlayCurrentTrack()
    {
        if (tracks.Count == 0 || tracks[currentTrackIndex] == null)
            return;

        AudioSource current = tracks[currentTrackIndex];
        current.volume = 1f;
        current.Play();

        if (trackNameText != null)
        {
            string trackName = current.clip != null ? current.clip.name : $"Track {currentTrackIndex + 1}";
            ShowTrackName(trackName);
        }
    }

    public void AdjustVolume(float delta)
    {
        if (tracks.Count == 0 || tracks[currentTrackIndex] == null)
            return;

        AudioSource current = tracks[currentTrackIndex];
        current.volume = Mathf.Clamp01(current.volume + delta);
    }

    public void ShowTrackName(string name)
    {
        if (trackNameText == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTrackName(name));
    }

    private IEnumerator FadeTrackName(string name)
    {
        trackNameText.text = name;
        Color color = trackNameText.color;
        color.a = 0f;
        trackNameText.color = color;

        // Fade in
        while (trackNameText.color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            trackNameText.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(displayTime);

        // Fade out
        while (trackNameText.color.a > 0f)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            trackNameText.color = color;
            yield return null;
        }
    }
}