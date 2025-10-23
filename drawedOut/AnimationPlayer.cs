namespace drawedOut
{
    public class AnimationPlayer
    {
        private Bitmap[] _animFrames;

        private int 
            _frameNo = 0,
            _totalFrameCount;

        private static readonly string 
            PROJ_PATH = Global.GetProjFolder(),
            SPRITE_FOLDER = @"sprites\";

        public int CurFrame { get => return _frameNo; }
        public int Length { get => return _totalFrameCount; }

        ///<summary>
        ///initalises an animationPlayer object with the animation frames in the given folder
        ///</summary>
        ///<param name="animationFolder"> the name of the folder within project/sprites/ </param>
        public AnimationPlayer(string animationFolder)
        {
            string animPath = Path.Combine(PROJ_PATH, SPRITE_FOLDER, animationFolder);

            if ( !Directory.Exists(animPath) )
                throw new DirectoryNotFoundException($"Directory {animPath} not found");
                    
            string[] fileNames = Directory.GetFiles(animPath);
            _totalFrameCount = fileNames.Count();

            _animFrames = new Bitmap[_totalFrameCount];

            for (int i = 0; i < _totalFrameCount; i++) { _animFrames[i] = new Bitmap(fileNames[i]); }
        }


        public Bitmap NextFrame()
        {
            Bitmap img = _animFrames[_frameNo];
            int size = img.Width / 10;

            if (++_frameNo >= _totalFrameCount) _frameNo = 0;

            return img;
        }
    }
}


