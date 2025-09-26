using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawedOut
{
    internal class Platform : Entity
    {
        /// <summary>
        /// 2D array: [level] [chunk]
        /// </summary>
        public static List<Platform>[][] PlatformList = new List<Platform>[TotalLevels][];

        static Platform()
        {
            // basically copied from character lol
            for (int level = 0; level < TotalLevels; level++) {
                int chunks = ChunksInLvl[level];
                PlatformList[level] = new List<Platform>[chunks];

                for (int i = 0; i < chunks; i++)
                {
                    PlatformList[level][i] = new List<Platform>();
                }
            }
        }



        public Platform(Point origin, int width, int height, int LocatedLevel, int LocatedChunk, bool isMainPlat=false)
            : base(origin, width, height, isMainPlat: isMainPlat)
        {
            PlatformList[LocatedLevel][LocatedChunk].Add(this);
        } 



        public PointF getPoint()
        {
            return (this.getLocation());
        }

    }
}
