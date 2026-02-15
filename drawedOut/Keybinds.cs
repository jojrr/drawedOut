namespace drawedOut
{
    internal static class Keybinds
    {
        public enum Actions { Jump, MoveLeft, MoveRight, Special1, Special2, Special3 };
        public static IReadOnlyDictionary<Keys, Actions> Bindings => _keyActionDict;
        public static IReadOnlyDictionary<Actions, Keys> ActionBindings => _actionKeyDict;

        private static Dictionary<Keys, Actions> _keyActionDict = new Dictionary<Keys, Actions>();
        private static Dictionary<Actions, Keys> _actionKeyDict = new Dictionary<Actions, Keys>();

        static Keybinds()
        {
            _keyActionDict = SaveData.GetKeybinds();
            if (_keyActionDict is null)
            {
                _keyActionDict = new Dictionary<Keys, Actions>
                {
                    { Keys.Space, Actions.Jump      },
                    { Keys.A,     Actions.MoveLeft  },
                    { Keys.D,     Actions.MoveRight },
                    { Keys.E,     Actions.Special1  },
                    { Keys.R,     Actions.Special2  },
                    { Keys.F,     Actions.Special3  },
                };
            }
            foreach (KeyValuePair<Keys, Actions> keyPair in _keyActionDict)
            { _actionKeyDict.Add(keyPair.Value, keyPair.Key); }
        }

        public static bool Rebind(Keys key, Actions action)
        {
            if (_keyActionDict.Keys.Contains(key)) return false;

            _keyActionDict.Remove(_actionKeyDict[action]);
            _actionKeyDict.Remove(action);
            _keyActionDict.Add(key, action);
            _actionKeyDict.Add(action, key);
            SaveData.SaveKeybinds(_keyActionDict);
            return true;
        }
    }
}
