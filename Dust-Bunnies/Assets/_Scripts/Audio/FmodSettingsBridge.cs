using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public static class FmodSettingsBridge
{
    // Cache handles so we don't look them up every frame.
    private static VCA _masterVca;
    private static VCA _musicVca;
    private static VCA _sfxVca;
    private static bool _initialized;

    // These must match your FMOD Studio VCA paths.
    private const string MasterPath = "vca:/Master";
    private const string MusicPath = "vca:/Music";
    private const string SfxPath = "vca:/SFX";

    private static void EnsureInit()
    {
        if (_initialized) return;

        // RuntimeManager should be available once FMOD has initialized.
        _masterVca = RuntimeManager.GetVCA(MasterPath);
        _musicVca = RuntimeManager.GetVCA(MusicPath);
        _sfxVca = RuntimeManager.GetVCA(SfxPath);

        _initialized = true;
    }

    /// <summary>
    /// Applies volume + mute to FMOD VCAs. Expects 0..1.
    /// </summary>
    public static void Apply(float master, float music, float sfx, bool muteAll)
    {
        EnsureInit();

        float m = Mathf.Clamp01(master);
        float mus = Mathf.Clamp01(music);
        float fx = Mathf.Clamp01(sfx);

        if (muteAll)
        {
            // simplest "mute": zero master
            _masterVca.setVolume(0f);
            return;
        }

        // Master drives overall loudness.
        _masterVca.setVolume(m);

        // You can set these either as raw (0..1) or multiplied by master.
        // If you already set master separately, DON'T multiply again here.
        _musicVca.setVolume(mus);
        _sfxVca.setVolume(fx);
    }
}
