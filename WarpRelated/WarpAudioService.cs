using StardewValley;

namespace Ts_Core.Services.WarpRelated
{
    /// <summary>
    /// Warp時のAudio Cue再生を管理します。
    /// </summary>
    internal static class WarpAudioService
    {
        //----------------------------------------
        // Audio Cue
        //----------------------------------------

        /// <summary>
        /// Audio Cueを指定回数・指定間隔・開始遅延で再生します。
        /// </summary>
        internal static void PlayAudioCue(
            string audioCue,
            int repeatCount,
            int intervalMs,
            int startDelayMs)
        {
            if (string.IsNullOrWhiteSpace(
                    audioCue))
            {
                return;
            }

            repeatCount =
                Math.Max(
                    1,
                    repeatCount);

            intervalMs =
                Math.Max(
                    0,
                    intervalMs);

            startDelayMs =
                Math.Max(
                    0,
                    startDelayMs);

            //----------------------------------------
            // 指定回数再生
            //----------------------------------------

            for (int i = 0;
                 i < repeatCount;
                 i++)
            {
                int delay =
                    startDelayMs
                    + i * intervalMs;

                //----------------------------------------
                // 即時再生
                //----------------------------------------

                if (delay <= 0)
                {
                    Game1.currentLocation?.playSound(
                        audioCue);

                    continue;
                }

                //----------------------------------------
                // 遅延再生
                //----------------------------------------

                DelayedAction.functionAfterDelay(
                    () =>
                    {
                        Game1.currentLocation?.playSound(
                            audioCue);
                    },
                    delay);
            }
        }
    }
}