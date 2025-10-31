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
                if ((value != 0.5) || (value != 1.0) || (value != 1.5))
                    throw new Exception("scale must be 0.5, 1.0, 1.5");

                _baseScale = value;
            }
        }


        private static float _leftScrollBound = 500;
        private static float _rightScrollBound = 1300;
        public static float LeftScrollBound { get => _leftScrollBound * BaseScale; }
        public static float RightScrollBound { get => _rightScrollBound * BaseScale; }



        // <summary>
        // Threshold for entities to be "active" (either side of screen center)
        // </summary>
        public static int EntityLoadThreshold
        {
            get => (int)(_levelBaseSize.Width*_baseScale);
        }


        private static Size _levelBaseSize = new Size(1860, 770);
        public static Size LevelSize 
        { 
            get 
            {
                SizeF floatP = new SizeF (_levelBaseSize.Width*_baseScale, _levelBaseSize.Height*_baseScale); 
                return Size.Truncate(floatP);
            }
        }

        private static Point _centerOfScreen;
        public static Point CenterOfScreen
        {
            get => _centerOfScreen;
        }

        public static void CalcNewCenter()
        {

            _centerOfScreen = new Point(
                    (int)(_levelBaseSize.Width/2*BaseScale), 
                    (int)(_levelBaseSize.Height/2*BaseScale));
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

