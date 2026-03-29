using System;
using System.Collections.Generic;
using Core;
using Game.Config;
using Game.Level.Inventory;
using Game.Level.Place;
using UnityEngine;

namespace Game.Domain
{
    [Serializable]
    public sealed class GameModel : Observable
    {
        public static GameModel Load(GameConfig config)
        {
            try
            {
                var data = PlayerPrefs.GetString("model");
                if (string.IsNullOrEmpty(data))
                {
                    var model = new GameModel();
                    model.Prepare(config);
                    return model;
                }
                var result = JsonUtility.FromJson<GameModel>(data);
                return result;
            }
            catch (Exception e)
            {
                PlayerPrefs.DeleteAll();
                Log.Exception(e);
                var model = new GameModel();
                model.Prepare(config);
                return model;
            }
        }

        public int Player;
        public long Cash;
        public int Hotel;
        public bool IsNoAds;
        public List<InventoryType> InventoryTypes;
        public bool JoystickVisibility;

        public GameModel()
        {
            InventoryTypes = new List<InventoryType>();
        }

        private void Prepare(GameConfig config)
        {
            Cash = config.DefaultCash;
            Hotel = config.DefaultHotel;
            IsNoAds = false;
            JoystickVisibility = false;
        }

        public void Save()
        {
            var data = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("model", data);
            PlayerPrefs.Save();
        }

        public void Remove()
        {
            PlayerPrefs.DeleteKey("model");
            PlayerPrefs.Save();
        }

        public string GenerateEntityID(int hotel, EntityType type, int number)
        {
            return "Hotel" + hotel + type.ToString() + number;
        }

        public int LoadLvl()
        {
            string hotelLvlWord = "HotelLvl";
            string key = hotelLvlWord + Hotel;
            return PlayerPrefs.GetInt(key, 1);
        }
        public void SaveLvl(int lvl)
        {
            string hotelLvlWord = "HotelLvl";
            string key = hotelLvlWord + Hotel;
            PlayerPrefs.SetInt(key, lvl);
            PlayerPrefs.Save();
        }

        public int LoadProgress()
        {
            string hotelProgressWord = "HotelProgress";
            string key = hotelProgressWord + Hotel;
            return PlayerPrefs.GetInt(key, 0);
        }
        public void SaveProgress(int progress)
        {
            string hotelProgressWord = "HotelProgress";
            string key = hotelProgressWord + Hotel;
            PlayerPrefs.SetInt(key, progress);
            PlayerPrefs.Save();
        }

        public bool LoadPlaceIsUsed(string id)
        {
            string isUsedWord = "IsUsed";
            string key = isUsedWord + id;
            int value = PlayerPrefs.GetInt(key, 0);
            if (value == 1) return true;
            else return false;
        }
        public void SavePlaceIsUsed(string id, int isUsed)
        {
            string isUsedWord = "IsUsed";
            string key = isUsedWord + id;
            PlayerPrefs.SetInt(key, isUsed);
            PlayerPrefs.Save();
        }

        public bool LoadPlaceIsPurchased(string id)
        {
            string areaOneID = GenerateEntityID(Hotel, EntityType.Area, 1);
            string roomZeroID = GenerateEntityID(Hotel, EntityType.Room, 0);

            string isPurchasedWord = "IsPurchased";
            string key = isPurchasedWord + id;
            int value = PlayerPrefs.GetInt(key, 0);
            if (roomZeroID == id || areaOneID == id || value == 1) return true;
            else return false;
        }
        public void SavePlaceIsPurchased(string id)
        {
            string isPurchasedWord = "IsPurchased";
            string key = isPurchasedWord + id;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        public void SavePlaceLvl(string id, int lvl)
        {
            string lvlWord = "Lvl";
            string key = lvlWord + id;
            PlayerPrefs.SetInt(key, lvl);
            PlayerPrefs.Save();
        }
        public int LoadPlaceLvl(string id)
        {
            string lvlWord = "Lvl";
            string key = lvlWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }


        public void SavePlaceVisualIndex(string id, int visualIndex)
        {
            string visualIndexWord = "VisualIndex";
            string key = visualIndexWord + id;
            PlayerPrefs.SetInt(key, visualIndex);
            PlayerPrefs.Save();
        }
        public int LoadPlaceVisualIndex(string id)
        {
            string visualIndexWord = "VisualIndex";
            string key = visualIndexWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }

        public void SavePlaceCash(string id, long cash)
        {
            string cashWord = "Cash";
            string key = cashWord + id;
            PlayerPrefs.SetString(key, cash.ToString());
            PlayerPrefs.Save();
        }
        public long LoadPlaceCash(string id)
        {
            string cashWord = "Cash";
            string key = cashWord + id;
            var value = PlayerPrefs.GetString(key, "0");
            return Convert.ToInt64(value);
        }

        public void SaveVisitsCount(string id, int numberOfVisits)
        {
            string visitsCountWord = "VisitsCount";
            string key = visitsCountWord + id;
            PlayerPrefs.SetInt(key, numberOfVisits);
            PlayerPrefs.Save();
        }
        public int LoadVisitsCount(string id)
        {
            string visitsCountWord = "VisitsCount";
            string key = visitsCountWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }


        public void SaveWatchAdsTimes()
        {
            var times = LoadWatchAdsTimes();
            times++;
            PlayerPrefs.SetInt(GameConstants.kWatchAdsTimes, times);
            PlayerPrefs.Save();
        }

        public int LoadWatchAdsTimes()
        {
            return PlayerPrefs.GetInt(GameConstants.kWatchAdsTimes, 0);
        }

        public int LoadLoginDays()
        {
            return PlayerPrefs.GetInt(GameConstants.kLoginDays, 1);
        }

        public void SaveLoginDay()
        {
            var days = LoadLoginDays();
            days++;
            PlayerPrefs.SetInt(GameConstants.kLoginDays, days);
            PlayerPrefs.Save();
        }

        public void ResetLoginDays()
        {
            PlayerPrefs.SetInt(GameConstants.kLoginDays, 1);
            PlayerPrefs.Save();
        }
    }
}