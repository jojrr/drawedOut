global using System.Collections.Immutable;
global using System.Diagnostics;
namespace drawedOut
{
    ///<summary>
    ///globally used static functions or values
    ///</summary>
    public static class Global
    {
        public const int 
            _GRAVITY = 4000,
            _DEFAULT_FONT_SIZE = 20,
            MAX_THREADS_TO_USE = 4;
        public const float 
            ZOOM_FACTOR = 1.05F,
            SLOW_FACTOR = 3.5F,
            SLOW_DURATION_S = 0.35F,
            FREEZE_DURATION_S = 0.15F,
            ANIMATION_FPS = 1000/24F;

        private static float _leftScrollBound = 0;
        private static float _rightScrollBound = 0;
        public static float LeftScrollBound { get => _leftScrollBound; }
        public static float RightScrollBound { get => _rightScrollBound; }

        /// <summary>
        /// Base gravity multiplied by the base scale.
        /// </summary>
        public static int Gravity { get => (int)(_GRAVITY*_baseScale); }

        /// <summary> Should the background be displayed </summary>
        public static bool ShowBG;

        /// <summary> Should the background be displayed </summary>
        public static bool ShowTime;

        /// <summary>
        /// Threshold for entities to be "active" (either side of screen center)
        /// </summary>
        public static int EntityLoadThreshold { get => (int)(_levelSize.Width*0.75); }

        /// <summary>
        /// logic for game framerate
        /// </summary>
        private static UInt16 _gameTickFreq = 60;
        public static UInt16 GameTickFreq 
        {
            get => _gameTickFreq;
            set 
            {
                _gameTickFreq = value; 
                if ( 1 > value) _gameTickFreq = 1;
                if ( 120 < value) _gameTickFreq = 120;
            }
        }

        public enum XDirections { left, right }
        public enum YDirections { top, bottom }
        public enum Resolutions { p720, p1080, p1440 }

        /// <summary>
        /// Immutable dict to store resolution sizes
        /// </summary>
        private static ImmutableDictionary<Resolutions,Size> ResDict = ImmutableDictionary.CreateRange 
            (new KeyValuePair<Resolutions,Size>[]{
            KeyValuePair.Create( Resolutions.p720,  new Size(1280, 720)  ),
            KeyValuePair.Create( Resolutions.p1080, new Size(1920, 1080) ),
            KeyValuePair.Create( Resolutions.p1440, new Size(2560, 1440) ),
            });

        private static float _baseScale = 1F;
        public static float BaseScale 
        { 
            get 
            {
                if (_baseScale == 0F) throw new Exception("Resolution not set");
                return _baseScale;
            }
        }

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
                        _baseScale = 2/3F;
                        break;
                    case Resolutions.p1080:
                        _baseScale = 1F;
                        break;
                    case Resolutions.p1440:
                        _baseScale = 4/3F;
                        break;
                }

                _leftScrollBound = (int)(scaleWidth * scrollBoundPercent);
                _rightScrollBound = (int)(scaleWidth * (1-scrollBoundPercent));
                CalcNewCenter();
            }
        }

        private static Size _levelSize = new Size(1920, 1080);
        public static Size LevelSize { get => _levelSize; }

        private static Point _centerOfScreen;
        public static Point CenterOfScreen { get => _centerOfScreen; }

        /// <summary> The default main font used in the game </summary>
        public static Font DefaultFont = new Font("Sour Gummy Black", _DEFAULT_FONT_SIZE*BaseScale);

        /// <summary> Calculates the new Center of the screen </summary>
        public static void CalcNewCenter()
        {
            _centerOfScreen = new Point(
                    _levelSize.Width/2,
                    _levelSize.Height/2);
        }

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

        /// <summary>
        /// Convert retrived image file into a bitmap with a standardised scaling used across the game.
        /// </summary>
        /// <param name="fileDirectory"> The full directory of the file to convert. </param>
        /// <returns> A <see cref="Bitmap"/>, in the shape of a square, scaled to fit the resolution </returns>
        public static Bitmap ImageToBitmap(string fileDirectory, 
                UInt16 spriteWidth=256, UInt16 spriteHeight=256)
        {
            return new Bitmap(
                    Image.FromFile(fileDirectory), 
                    (int)(spriteWidth*Global.BaseScale), 
                    (int)(spriteHeight*Global.BaseScale)
                    );
        }

        /// <summary>
        /// get a single image from the specified folder.
        /// </summary>
        /// <param name="folderName"> the name of the folder stored within the "sprites/" directory </param>
        /// <param name="fileName"> 
        /// The name of the file within the specified folder <br/>
        /// Default: null (returns first item) 
        /// </param>
        /// <returns> 
        /// A single <see cref="Bitmap"/> image of the specified file in the folder.<br/>
        /// By default if fileName is unset, will return the first item in the folder.
        /// </returns>
        public static Bitmap GetSingleImage(string folderName, 
                string? fileName=null, UInt16 spriteWidth=256, UInt16 spriteHeight=256)
        {
            string filePath = Path.Combine(GetProjFolder(), @"sprites\", folderName);

            if (fileName is null) 
            {
                fileName = Directory.GetFiles(filePath)[0];
                return ImageToBitmap(fileName, spriteWidth, spriteHeight);
            }

            filePath = Path.Combine(filePath, fileName);
            return ImageToBitmap(filePath, spriteWidth, spriteHeight);
        }

    }
}

