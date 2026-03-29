using System;
using System.Collections.Generic;
using Core;
using Game.Config;
using Game.Level.Inventory;
using Game.Level.Place;
using UnityEngine;

namespace Game.Domain
{
    /// <summary>
    /// GameModel holds ALL of the player's saved game data (cash, hotel level, purchases, etc.).
    /// It knows how to save and load itself using Unity's PlayerPrefs system.
    ///
    /// [Serializable] is an ATTRIBUTE that tells C# this class can be converted
    /// to/from a data format (like JSON). This is needed for saving and loading.
    ///
    /// "Observable" is the parent class -- it likely implements the OBSERVER PATTERN,
    /// which lets other objects watch for changes to this model's data.
    /// </summary>
    [Serializable]
    public sealed class GameModel : Observable
    {
        [NonSerialized] private bool _isDirty;

        /// <summary>
        /// Marks that PlayerPrefs has pending changes that need flushing to disk.
        /// The actual PlayerPrefs.Save() is deferred to FlushIfDirty().
        /// </summary>
        private void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Writes pending PlayerPrefs changes to disk if any exist.
        /// Call this periodically (e.g., every few seconds) and on app pause/quit.
        /// </summary>
        public void FlushIfDirty()
        {
            if (!_isDirty) return;
            _isDirty = false;
            PlayerPrefs.Save();
        }

        /// <summary>
        /// A STATIC method belongs to the CLASS, not to any specific instance.
        /// You call it as GameModel.Load(config) without creating a GameModel first.
        /// This is a FACTORY METHOD -- it creates and returns a GameModel for you.
        /// It tries to load saved data; if none exists or loading fails, it creates a fresh model.
        /// </summary>
        public static GameModel Load(GameConfig config)
        {
            // try/catch is ERROR HANDLING. Code in "try" runs normally.
            // If something goes wrong (an Exception), the "catch" block runs instead,
            // preventing the game from crashing.
            try
            {
                // PlayerPrefs is Unity's simple key-value storage system.
                // It saves small amounts of data (like settings and progress) to disk.
                var data = PlayerPrefs.GetString("model");
                // Check if there is no saved data
                if (string.IsNullOrEmpty(data))
                {
                    // No save found -- create a brand new model with default values
                    var model = new GameModel();
                    model.Prepare(config);
                    return model;
                }
                // JsonUtility.FromJson DESERIALIZES the JSON string back into a GameModel object.
                // JSON is a text format like: {"Cash":100,"Hotel":1}
                var result = JsonUtility.FromJson<GameModel>(data);
                return result;
            }
            catch (Exception e)
            {
                // Something went wrong with the saved data (corrupted, wrong format, etc.)
                // Delete all saved data and start fresh to recover gracefully.
                PlayerPrefs.DeleteAll();
                Log.Exception(e);
                var model = new GameModel();
                model.Prepare(config);
                return model;
            }
        }

        // PUBLIC FIELDS -- these are the actual saved data.
        // "public" means any other class can read and write these directly.
        public int Player;                          // Which player character/skin is selected
        public long Cash;                           // Player's in-game currency ("long" holds very large numbers)
        public int Hotel;                           // Which hotel/level the player is currently on
        public bool IsNoAds;                        // Whether the player purchased "remove ads"
        public List<InventoryType> InventoryTypes;  // Items the player owns
        public bool JoystickVisibility;             // Whether the on-screen joystick is visible

        /// <summary>
        /// Constructor -- called when you write "new GameModel()".
        /// Initializes the inventory list so it's not null.
        /// </summary>
        public GameModel()
        {
            InventoryTypes = new List<InventoryType>();
        }

        /// <summary>
        /// Sets up a new model with default values from the game configuration.
        /// Called when no saved data exists or when saved data is corrupted.
        /// </summary>
        private void Prepare(GameConfig config)
        {
            Cash = config.DefaultCash;
            Hotel = config.DefaultHotel;
            IsNoAds = false;
            JoystickVisibility = false;
        }

        /// <summary>
        /// SERIALIZES this model to JSON and saves it to PlayerPrefs.
        /// "this" refers to the current object instance.
        /// JsonUtility.ToJson converts the object into a JSON string for storage.
        /// </summary>
        public void Save()
        {
            var data = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("model", data);
            MarkDirty();
        }

        /// <summary>
        /// Deletes the saved model data from PlayerPrefs.
        /// Use this to reset the player's progress.
        /// </summary>
        public void Remove()
        {
            PlayerPrefs.DeleteKey("model");
            MarkDirty();
        }

        /// <summary>
        /// Creates a unique string ID for a game entity (like a room or area).
        /// STRING CONCATENATION with "+" joins multiple strings together.
        /// Example result: "Hotel1Room3"
        /// ToString() converts a non-string value (like an enum) to its text representation.
        /// </summary>
        public string GenerateEntityID(int hotel, EntityType type, int number)
        {
            return "Hotel" + hotel + type.ToString() + number;
        }

        /// <summary>
        /// Loads the current hotel's level from PlayerPrefs.
        /// The second argument to GetInt (1) is the DEFAULT VALUE returned
        /// when no saved data exists for that key.
        /// </summary>
        public int LoadLvl()
        {
            string hotelLvlWord = "HotelLvl";
            string key = hotelLvlWord + Hotel; // e.g., "HotelLvl2"
            return PlayerPrefs.GetInt(key, 1);
        }

        /// <summary>
        /// Saves the current hotel's level to PlayerPrefs.
        /// </summary>
        public void SaveLvl(int lvl)
        {
            string hotelLvlWord = "HotelLvl";
            string key = hotelLvlWord + Hotel;
            PlayerPrefs.SetInt(key, lvl);
            MarkDirty();
        }

        /// <summary>
        /// Loads the player's progress percentage for the current hotel.
        /// Returns 0 by default if nothing is saved.
        /// </summary>
        public int LoadProgress()
        {
            string hotelProgressWord = "HotelProgress";
            string key = hotelProgressWord + Hotel;
            return PlayerPrefs.GetInt(key, 0);
        }

        /// <summary>
        /// Saves the player's progress for the current hotel.
        /// </summary>
        public void SaveProgress(int progress)
        {
            string hotelProgressWord = "HotelProgress";
            string key = hotelProgressWord + Hotel;
            PlayerPrefs.SetInt(key, progress);
            MarkDirty();
        }

        /// <summary>
        /// Checks if a place (room, area, etc.) is currently being used.
        /// PlayerPrefs stores integers, so we use 1 for true and 0 for false.
        /// </summary>
        public bool LoadPlaceIsUsed(string id)
        {
            string isUsedWord = "IsUsed";
            string key = isUsedWord + id; // e.g., "IsUsedHotel1Room2"
            int value = PlayerPrefs.GetInt(key, 0);
            if (value == 1) return true;
            else return false;
        }

        /// <summary>
        /// Saves whether a place is being used (1 = used, 0 = not used).
        /// </summary>
        public void SavePlaceIsUsed(string id, int isUsed)
        {
            string isUsedWord = "IsUsed";
            string key = isUsedWord + id;
            PlayerPrefs.SetInt(key, isUsed);
            MarkDirty();
        }

        /// <summary>
        /// Checks if a place has been purchased by the player.
        /// Some places are free by default: Area #1 and Room #0 are always "purchased".
        /// The "||" operator means OR -- if ANY condition is true, the result is true.
        /// </summary>
        public bool LoadPlaceIsPurchased(string id)
        {
            // Generate IDs for the places that are always purchased (free starter places)
            string areaOneID = GenerateEntityID(Hotel, EntityType.Area, 1);
            string roomZeroID = GenerateEntityID(Hotel, EntityType.Room, 0);

            string isPurchasedWord = "IsPurchased";
            string key = isPurchasedWord + id;
            int value = PlayerPrefs.GetInt(key, 0);
            // Return true if it's a free starter place OR if the player bought it
            if (roomZeroID == id || areaOneID == id || value == 1) return true;
            else return false;
        }

        /// <summary>
        /// Marks a place as purchased and saves it.
        /// </summary>
        public void SavePlaceIsPurchased(string id)
        {
            string isPurchasedWord = "IsPurchased";
            string key = isPurchasedWord + id;
            PlayerPrefs.SetInt(key, 1);
            MarkDirty();
        }

        /// <summary>
        /// Saves the upgrade level of a specific place.
        /// </summary>
        public void SavePlaceLvl(string id, int lvl)
        {
            string lvlWord = "Lvl";
            string key = lvlWord + id;
            PlayerPrefs.SetInt(key, lvl);
            MarkDirty();
        }

        /// <summary>
        /// Loads the upgrade level of a specific place (defaults to 0).
        /// </summary>
        public int LoadPlaceLvl(string id)
        {
            string lvlWord = "Lvl";
            string key = lvlWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }


        /// <summary>
        /// Saves which visual style/appearance a place is using.
        /// </summary>
        public void SavePlaceVisualIndex(string id, int visualIndex)
        {
            string visualIndexWord = "VisualIndex";
            string key = visualIndexWord + id;
            PlayerPrefs.SetInt(key, visualIndex);
            MarkDirty();
        }

        /// <summary>
        /// Loads the visual style index for a place (defaults to 0).
        /// </summary>
        public int LoadPlaceVisualIndex(string id)
        {
            string visualIndexWord = "VisualIndex";
            string key = visualIndexWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }

        /// <summary>
        /// Saves how much cash a specific place has earned/accumulated.
        /// Uses SetString because "long" values can be too large for SetInt.
        /// ToString() converts the number to a string for storage.
        /// </summary>
        public void SavePlaceCash(string id, long cash)
        {
            string cashWord = "Cash";
            string key = cashWord + id;
            PlayerPrefs.SetString(key, cash.ToString());
            MarkDirty();
        }

        /// <summary>
        /// Loads how much cash a specific place has earned.
        /// Convert.ToInt64 parses a string back into a long (64-bit integer).
        /// </summary>
        public long LoadPlaceCash(string id)
        {
            string cashWord = "Cash";
            string key = cashWord + id;
            var value = PlayerPrefs.GetString(key, "0");
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Saves how many times a place has been visited by guests.
        /// </summary>
        public void SaveVisitsCount(string id, int numberOfVisits)
        {
            string visitsCountWord = "VisitsCount";
            string key = visitsCountWord + id;
            PlayerPrefs.SetInt(key, numberOfVisits);
            MarkDirty();
        }

        /// <summary>
        /// Loads the visit count for a place (defaults to 0).
        /// </summary>
        public int LoadVisitsCount(string id)
        {
            string visitsCountWord = "VisitsCount";
            string key = visitsCountWord + id;
            return PlayerPrefs.GetInt(key, 0);
        }


        /// <summary>
        /// Increments and saves the total number of rewarded ads the player has watched.
        /// The "++" operator adds 1 to a number (times = times + 1).
        /// </summary>
        public void SaveWatchAdsTimes()
        {
            var times = LoadWatchAdsTimes();
            times++;
            PlayerPrefs.SetInt(GameConstants.kWatchAdsTimes, times);
            MarkDirty();
        }

        /// <summary>
        /// Returns how many rewarded ads the player has watched total.
        /// GameConstants.kWatchAdsTimes is a CONSTANT -- a value that never changes,
        /// used here as the PlayerPrefs key name.
        /// </summary>
        public int LoadWatchAdsTimes()
        {
            return PlayerPrefs.GetInt(GameConstants.kWatchAdsTimes, 0);
        }

        /// <summary>
        /// Returns how many consecutive days the player has logged in.
        /// Defaults to 1 (first day).
        /// </summary>
        public int LoadLoginDays()
        {
            return PlayerPrefs.GetInt(GameConstants.kLoginDays, 1);
        }

        /// <summary>
        /// Increments the login day counter by 1 and saves it.
        /// </summary>
        public void SaveLoginDay()
        {
            var days = LoadLoginDays();
            days++;
            PlayerPrefs.SetInt(GameConstants.kLoginDays, days);
            MarkDirty();
        }

        /// <summary>
        /// Resets the login day counter back to 1 (e.g., when the player misses a day).
        /// </summary>
        public void ResetLoginDays()
        {
            PlayerPrefs.SetInt(GameConstants.kLoginDays, 1);
            MarkDirty();
        }
    }
}
