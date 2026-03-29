using System;
using Game.Domain;
using UnityEngine;

namespace Game.Managers
{
    public sealed class LoginManager
    {
        public void Initialize(GameModel gameModel)
        {
            var lastLoginDate = LoadLastLoginDate();
            var nowDate = DateTime.Now;

            TimeSpan timePassed = nowDate - lastLoginDate;

            var deltaDays = timePassed.Days;
            var hours = timePassed.Hours;

            if (deltaDays == 1)
            {
                gameModel.SaveLoginDay();
                SaveLoginDate(DateTime.Now.ToString());
            }
            else if (deltaDays > 1)
            {
                gameModel.ResetLoginDays();
                SaveLoginDate(DateTime.Now.ToString());
            }

            //Log.Info("Last Login Date: " + lastLoginDate + " Time Passed Hours: " + hours + " Time Passed Days: " + deltaDays);
        }

        private DateTime LoadLastLoginDate()
        {
            string date = PlayerPrefs.GetString(GameConstants.kLoginDate, string.Empty);
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString();
                SaveLoginDate(date);
            }
            return DateTime.Parse(date);
        }

        private void SaveLoginDate(string date)
        {
            PlayerPrefs.SetString(GameConstants.kLoginDate, date);
            PlayerPrefs.Save();
        }
    }
}

