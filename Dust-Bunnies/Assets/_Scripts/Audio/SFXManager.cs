using UnityEngine;

public static class SFXManager
{
    public static class Events
    {
        public const uint SparklerLoop = 3667990691;
        public const uint Footstep = 3854155799;
    }

    // Play and return the playing ID (so you can stop it later)
    public static uint PlaySFX(uint sfxEvent, GameObject source = null)
    {
        GameObject target = source != null ? source : Camera.main.gameObject;
        return AkUnitySoundEngine.PostEvent(sfxEvent, target);
    }

    // Stop a specific playing instance
    public static void StopSFX(uint playingId, int fadeOutMs = 200)
    {
        AkUnitySoundEngine.StopPlayingID(playingId, fadeOutMs);
    }

    // Set an RTPC value
    public static void SetParam(string rtpcName, float value, GameObject source = null)
    {
        GameObject target = source != null ? source : Camera.main.gameObject;
        AkUnitySoundEngine.SetRTPCValue(rtpcName, value, target);
    }
}