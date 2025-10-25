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



        private static int _entityLoadThreshold = 1000;
        public static int EntityLoadThreshold
        {
            get => (int)(_entityLoadThreshold*BaseScale);
        }


        public static Size _baseSize = new Size(1860, 770);
        public static Size LevelBaseSize { get => _baseSize; }

        private static Point _centerOfScreen;
        public static Point CenterOfScreen
        {
            get => _centerOfScreen;
        }

        public static void CalcNewCenter()
        {

            _centerOfScreen = new Point(
                    (int)(_baseSize.Width/2*BaseScale), 
                    (int)(_baseSize.Height/2*BaseScale));
        }

        public enum XDirections { left, right }
        public enum YDirections { top, bottom }

        /// <summary>
        /// Returns the current project's working directory that contains .csproj
        /// </summary>
        /// <returns> String: path to project's directory </returns>
        public static string GetProjFolder()
        {
            DirectoryInfo? dir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent;
            if (dir is null) throw new DirectoryNotFoundException("csproj directory not found");
            return dir.FullName;
        }
    }
}

