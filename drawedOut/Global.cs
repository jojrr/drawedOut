namespace drawedOut
{
    ///<summary>
    ///globally used static functions or values
    ///</summary>
    public static class Global
    {
        public const int MAX_THREADS_TO_USE = 4;
        public const float 
            ZOOM_FACTOR = 1.05F,
            SLOW_FACTOR = 3.5F,
            SLOW_DURATION_S = 0.35F,
            FREEZE_DURATION_S = 0.15F,
            ANIMATION_FPS = 1000/24F;

        private static int _gameTickFreq = 60;
        public static int GameTickFreq 
        {
            get  
            {
                if (_curResolution == Resolutions.p4k && _gameTickFreq > 60) return 60;
                return _gameTickFreq; 
            }
            set 
            {
                if ( 0 > value) _gameTickFreq = 0;
                if ( 120 < value) _gameTickFreq = 120;
                _gameTickFreq = value; 
            }
        }

        private static float _baseScale = 1F;
        public static float BaseScale 
        { 
            get 
            {
                if (_baseScale == 0F) throw new Exception("Resolution not set");
                return _baseScale;
            }
        }

        public enum Resolutions { p720, p1080, p1440, p4k }

        private static Dictionary<Resolutions,Size> ResDict = new Dictionary<Resolutions,Size> ()
        {
            { Resolutions.p720, new Size(1280, 720) },
            { Resolutions.p1080, new Size(1920, 1080) },
            { Resolutions.p1440, new Size(2560, 1440) },
            { Resolutions.p4k, new Size(3840, 2160) }
        };

        private static Resolutions _curResolution = Resolutions.p1080;
        public static Resolutions LevelResolution
        {
            get => _curResolution;
            set
            {
                _curResolution = value;
                _levelSize = ResDict[value];
                float scaleWidth = _levelSize.Width;
                float scrollBoundPercent = 0.2F;

                switch (value)
                {
                    case Resolutions.p720:
                        _baseScale = 1/3F;
                        break;
                    case Resolutions.p1080:
                        _baseScale = 1F;
                        break;
                    case Resolutions.p1440:
                        _baseScale = 4/3F;
                        break;
                    case Resolutions.p4k:
                        _baseScale = 4/3F;
                        scaleWidth *= 2/3F;
                        break;
                }

                _leftScrollBound = (int)(scaleWidth * scrollBoundPercent);
                _rightScrollBound = (int)(scaleWidth * (1-scrollBoundPercent));
                CalcNewCenter();
            }
        }

        private static float _leftScrollBound = 0;
        private static float _rightScrollBound = 0;
        public static float LeftScrollBound { get => _leftScrollBound; }
        public static float RightScrollBound { get => _rightScrollBound; }


        // <summary>
        // Threshold for entities to be "active" (either side of screen center)
        // </summary>
        public static int EntityLoadThreshold { get => (int)(_levelSize.Width/1.5); }


        private static Size _levelSize = new Size(1920, 1080);
        public static Size LevelSize { get => _levelSize; }

        private static Point _centerOfScreen;
        public static Point CenterOfScreen { get => _centerOfScreen; }

        public static void CalcNewCenter()
        {

            _centerOfScreen = new Point(
                    _levelSize.Width/2,
                    _levelSize.Height/2);
        }

        public enum XDirections { left, right }
        public enum YDirections { top, bottom }

        /// <summary>
        /// Returns the current project's working directory that contains .csproj
        /// </summary>
        /// <returns> String: path to project's directory </returns>
        public static string GetProjFolder()
        {
            DirectoryInfo? dir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent;
            if (dir is null) throw new DirectoryNotFoundException("csproj directory not found");
            return dir.FullName;
        }
    }
}

