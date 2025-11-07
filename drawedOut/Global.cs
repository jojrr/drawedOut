namespace drawedOut
{
    ///<summary>
    ///globally used static functions or values
    ///</summary>
    public static class Global
    {
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
        private static Resolutions _curResolution = Resolutions.p1080;
        public static Resolutions LevelResolution
        {
            get => _curResolution;
            set
            {
                _curResolution = value;

                float scaleF = 1F;
                switch (value)
                {
                    case Resolutions.p720:
                        scaleF = 1/3F;
                        break;
                    case Resolutions.p1080:
                        scaleF = 1F;
                        break;
                    case Resolutions.p1440:
                        scaleF = 4/3F;
                        break;
                    case Resolutions.p4k:
                        scaleF = 2F;
                        break;
                }

                SizeF floatSize = new SizeF (_levelSize.Width*scaleF, _levelSize.Height*scaleF); 
                _levelSize = Size.Truncate(floatSize);
                _rightScrollBound = (int)(floatSize.Width * 0.8);
                _leftScrollBound = (int)(floatSize.Width * 0.2);
                _baseScale = scaleF;
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

