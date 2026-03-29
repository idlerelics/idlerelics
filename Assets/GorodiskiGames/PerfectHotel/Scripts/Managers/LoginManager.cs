using System;
using Game.Domain;
using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// Tracks daily login streaks for the player.
    /// Compares the current date to the last saved login date:
    /// - If exactly 1 day has passed: increment the streak (consecutive login days)
    /// - If more than 1 day has passed: reset the streak (the player missed a day)
    /// - If same day: do nothing (already counted today)
    ///
    /// Uses PlayerPrefs for persistent storage -- data survives app restarts.
    /// PlayerPrefs stores key-value pairs (like a simple dictionary) on the device.
    /// </summary>
    public sealed class LoginManager
    {
        /// <summary>
        /// Called during game startup. Checks the time since the last login
        /// and updates the streak counter accordingly.
        ///
        /// TimeSpan represents the difference between two DateTime values.
        /// TimeSpan.Days gives the number of full days between the two dates.
        /// </summary>
        public void Initialize(GameModel gameModel)
        {
            var lastLoginDate = LoadLastLoginDate();
            var nowDate = DateTime.Now; // Current date and time

            TimeSpan timePassed = nowDate - lastLoginDate; // How long since last login

            var deltaDays = timePassed.Days;   // Number of full days
            var hours = timePassed.Hours;       // Remaining hours (not used but available)

            if (deltaDays == 1)
            {
                // Exactly 1 day passed -- the player logged in on consecutive days!
                gameModel.SaveLoginDay();                  // Increment the streak counter
                SaveLoginDate(DateTime.Now.ToString());    // Update the saved date
            }
            else if (deltaDays > 1)
            {
                // More than 1 day passed -- the streak is broken
                gameModel.ResetLoginDays();                // Reset streak to 0
                SaveLoginDate(DateTime.Now.ToString());
            }
            // If deltaDays == 0, same day login -- do nothing
        }

        /// <summary>
        /// Loads the last login date from PlayerPrefs.
        /// If no date was previously saved (first launch), saves today's date.
        /// DateTime.Parse converts a date string back to a DateTime object.
        /// </summary>
        private DateTime LoadLastLoginDate()
        {
            string date = PlayerPrefs.GetString(GameConstants.kLoginDate, string.Empty);
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString(); // First launch -- save today
                SaveLoginDate(date);
            }
            return DateTime.Parse(date);
        }

        /// <summary>
        /// Saves the login date to PlayerPrefs.
        /// PlayerPrefs.Save() ensures the data is written to disk immediately
        /// (otherwise it might only save when the app closes).
        /// </summary>
        private void SaveLoginDate(string date)
        {
            PlayerPrefs.SetString(GameConstants.kLoginDate, date);
            PlayerPrefs.Save();
        }
    }
}
