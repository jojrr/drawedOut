namespace drawedOut
{
    internal static class LevelRanks
    {
        public static Bitmap S, A, B, C, D;

        static LevelRanks()
        {
            S = Global.GetSingleImage(@"fillerPic\");
            A = Global.GetSingleImage(@"fillerPic\");
            B = Global.GetSingleImage(@"fillerPic\");
            C = Global.GetSingleImage(@"fillerPic\");
            D = Global.GetSingleImage(@"fillerPic\");
        }
    }
}
