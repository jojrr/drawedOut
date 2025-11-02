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
            get => _baseScale;
            set 
            {
                if (value == _baseScale) return;

                if ((value != 0.5) || (value != 1.0) || (value != 2.0))
                    throw new Exception("scale must be 0.5, 1.0, 1.5");

                SizeF floatSize = new SizeF (_levelSize.Width*_baseScale, _levelSize.Height*_baseScale); 
                _levelSize = Size.Truncate(floatSize);
                _leftScrollBound = (int)(_levelSize.Width * 0.2);
                _rightScrollBound = (int)(_levelSize.Width * 0.8);

                _baseScale = value;
            }
        }


        private static float _leftScrollBound = 512;
        private static float _rightScrollBound = 2048;
        public static float LeftScrollBound { get => _leftScrollBound; }
        public static float RightScrollBound { get => _rightScrollBound; }



        // <summary>
        // Threshold for entities to be "active" (either side of screen center)
        // </summary>
        public static int EntityLoadThreshold { get => (int)(_levelSize.Width/1.5*_baseScale); }


        private static Size _levelSize = new Size(2560, 1440);
        public static Size LevelSize { get => _levelSize; }

        private static Point _centerOfScreen;
        public static Point CenterOfScreen
        {
            get => _centerOfScreen;
        }

        public static void CalcNewCenter()
        {

            _centerOfScreen = new Point(
                    (int)(_levelSize.Width/2*BaseScale), 
                    (int)(_levelSize.Height/2*BaseScale));
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

