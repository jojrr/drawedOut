namespace drawedOut
{
    public class AnimationPlayer
    {
        private Bitmap[] _animFrames;

        private int 
            _frameNo = 0,
            _totalFrameCount;

        private const string SPRITE_FOLDER = @"sprites\";
        private static readonly string PROJ_PATH = Global.GetProjFolder();

        public int CurFrame { get => _frameNo; }
        public int LastFrame { get => _totalFrameCount-1; }

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
            for (int i = 0; i < _totalFrameCount; i++) 
            { _animFrames[i] = Global.ImageToBitmap(fileNames[i]); }
        }


        public Bitmap NextFrame(Global.XDirections facingDir = Global.XDirections.right)
        {
            Bitmap img;

            if (_totalFrameCount == 1) 
            {
                img = (Bitmap)(_animFrames[0].Clone());
                if (facingDir == Global.XDirections.left) img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                return img;
            }

            img = (Bitmap)(_animFrames[_frameNo].Clone());
            if (++_frameNo >= _totalFrameCount) ResetAnimation();
            if (facingDir == Global.XDirections.left) img.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return img;
        }

        public Bitmap GetFrame(int frameNo) => _animFrames[frameNo];

        public void ResetAnimation() => _frameNo = 0;
    }
}


