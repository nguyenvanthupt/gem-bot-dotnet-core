
using Sfs2X.Entities.Data;

namespace bot
{
    public class Grid
    {
        private List<Gem> gems = new List<Gem>();
        private ISFSArray gemsCode;
        public HashSet<GemType> gemTypes = new HashSet<GemType>();
        private HashSet<GemType> myHeroGemType;

        public Grid(ISFSArray gemsCode, ISFSArray gemModifiers, HashSet<GemType> gemTypes)
        {
            updateGems(gemsCode, gemModifiers);
            this.myHeroGemType = gemTypes;
        }

        public void updateGems(ISFSArray gemsCode, ISFSArray gemModifiers)
        {
            gems.Clear();
            gemTypes.Clear();
            for (int i = 0; i < gemsCode.Size(); i++)
            {
                Gem gem = new Gem(i, (GemType)gemsCode.GetByte(i), gemModifiers != null ? (GemModifier)gemModifiers.GetByte(i) : GemModifier.NONE);
                gems.Add(gem);
                gemTypes.Add(gem.type);
            }
        }

        public Pair<int> recommendSwapGem()
        {
            List<GemSwapInfo> listMatchGem = suggestMatch();

            Console.WriteLine("recommendSwapGem " + listMatchGem.Count);
            if (listMatchGem.Count == 0)
            {
                return new Pair<int>(-1, -1);
            }

            GemSwapInfo matchGemSizeThanFour = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4).FirstOrDefault();
            if (matchGemSizeThanFour != null)
            {
                return matchGemSizeThanFour.getIndexSwapGem();
            }
            GemSwapInfo matchGemSizeThanThree = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 3).FirstOrDefault();
            if (matchGemSizeThanThree != null)
            {
                return matchGemSizeThanThree.getIndexSwapGem();
            }
            GemSwapInfo matchGemSword = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();
            if (matchGemSword != null)
            {
                return matchGemSword.getIndexSwapGem();
            }

            foreach (GemType type in myHeroGemType)
            {
                GemSwapInfo matchGem = listMatchGem.Where(gemMatch => gemMatch.type == type).FirstOrDefault();
                //listMatchGem.stream().filter(gemMatch -> gemMatch.getType() == type).findFirst();
                if (matchGem != null)
                {
                    return matchGem.getIndexSwapGem();
                }
            }
            return listMatchGem[0].getIndexSwapGem();
        }

        private List<GemSwapInfo> suggestMatch()
        {
            var listMatchGem = new List<GemSwapInfo>();

            var tempGems = new List<Gem>(gems);
            foreach (Gem currentGem in tempGems)
            {
                Gem swapGem = null;
                // If x > 0 => swap left & check
                if (currentGem.x > 0)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x - 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If x < 7 => swap right & check
                if (currentGem.x < 7)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x + 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y < 7 => swap up & check
                if (currentGem.y < 7)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y + 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y > 0 => swap down & check
                if (currentGem.y > 0)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y - 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
            }
            return listMatchGem;
        }

        private void checkMatchSwapGem(List<GemSwapInfo> listMatchGem, Gem currentGem, Gem swapGem)
        {
            swap(currentGem, swapGem);
            HashSet<Gem> matchGems = matchesAt(currentGem.x, currentGem.y);

            swap(currentGem, swapGem);
            if (matchGems.Count > 0)
            {
                listMatchGem.Add(new GemSwapInfo(currentGem.index, swapGem.index, matchGems.Count, currentGem.type));
            }
        }

        private int getGemIndexAt(int x, int y)
        {
            return x + y * 8;
        }

        private void swap(Gem a, Gem b)
        {
            int tempIndex = a.index;
            int tempX = a.x;
            int tempY = a.y;

            // update reference
            gems[a.index] = b;
            gems[b.index] = a;

            // update data of element
            a.index = b.index;
            a.x = b.x;
            a.y = b.y;

            b.index = tempIndex;
            b.x = tempX;
            b.y = tempY;
        }

        private HashSet<Gem> matchesAt(int x, int y)
        {
            HashSet<Gem> res = new HashSet<Gem>();
            Gem center = gemAt(x, y);
            if (center == null)
            {
                return res;
            }

            // check horizontally
            List<Gem> hor = new List<Gem>();
            hor.Add(center);
            int xLeft = x - 1, xRight = x + 1;
            while (xLeft >= 0)
            {
                Gem gemLeft = gemAt(xLeft, y);
                if (gemLeft != null)
                {
                    if (!gemLeft.sameType(center))
                    {
                        break;
                    }
                    hor.Add(gemLeft);
                }
                xLeft--;
            }
            while (xRight < 8)
            {
                Gem gemRight = gemAt(xRight, y);
                if (gemRight != null)
                {
                    if (!gemRight.sameType(center))
                    {
                        break;
                    }
                    hor.Add(gemRight);
                }
                xRight++;
            }
            if (hor.Count >= 3)
            {
                res.UnionWith(hor);
                var ls = CoverGems(hor, "x");
                Console.WriteLine(ls.Count);
                foreach (var item in ls)
                {
                    res.UnionWith(item);
                }
            }

            // check vertically
            List<Gem> ver = new List<Gem>();
            ver.Add(center);
            int yBelow = y - 1, yAbove = y + 1;
            while (yBelow >= 0)
            {
                Gem gemBelow = gemAt(x, yBelow);
                if (gemBelow != null)
                {
                    if (!gemBelow.sameType(center))
                    {
                        break;
                    }
                    ver.Add(gemBelow);
                }
                yBelow--;
            }
            while (yAbove < 8)
            {
                Gem gemAbove = gemAt(x, yAbove);
                if (gemAbove != null)
                {
                    if (!gemAbove.sameType(center))
                    {
                        break;
                    }
                    ver.Add(gemAbove);
                }
                yAbove++;
            }
            if (ver.Count >= 3)
            {
                res.UnionWith(ver);
                var ls = CoverGems(ver, "y");
                Console.WriteLine(ls.Count);
                foreach (var item in ls)
                {
                    res.UnionWith(item);
                }
            }

            return res;
        }
        private List<List<Gem>> CoverGems(List<Gem> lsGems, string lsXY)
        {
            var cGems = new List<List<Gem>>();
            foreach (var gem in lsGems)
            {
                if (lsXY.Equals("x"))
                {
                    var ls1 = new List<Gem>();
                    var ls2 = new List<Gem>();
                    if (gem.y > 0)
                    {
                        ls1 = CoverGemCenter(gemAt(gem.x, gem.y - 1), "bellow");
                    }
                    if (gem.y < 7)
                    {
                        ls2 = CoverGemCenter(gemAt(gem.x, gem.y + 1), "above");
                    }
                    if (gemAt(gem.x, gem.y + 1) == gemAt(gem.x, gem.y - 1) && (ls1.Count + ls2.Count >= 3))
                    {
                        var ls = ls1.Union(ls2) as List<Gem>;
                        cGems.Add(ls);
                        continue;
                    }
                    if (ls1.Count >= 3) cGems.Add(ls1);
                    if (ls2.Count >= 3) cGems.Add(ls2);
                }
                else if (lsXY.Equals("y"))
                {
                    var ls1 = new List<Gem>();
                    var ls2 = new List<Gem>();
                    if (gem.x > 0)
                    {
                        ls1 = CoverGemCenter(gemAt(gem.x - 1, gem.y), "left");
                    }
                    if (gem.x < 7)
                    {
                        ls2 = CoverGemCenter(gemAt(gem.x + 1, gem.y), "right");
                    }
                    if (gemAt(gem.x - 1, gem.y) == gemAt(gem.x + 1, gem.y) && (ls1.Count + ls2.Count >= 3))
                    {
                        var ls = ls1.Union(ls2) as List<Gem>;
                        cGems.Add(ls);
                        continue;
                    }
                    if (ls1.Count >= 3) cGems.Add(ls1);
                    if (ls2.Count >= 3) cGems.Add(ls2);
                }
            }
            return cGems;
        }
        private List<Gem> CoverGemCenter(Gem center, string lsXY)
        {
            List<Gem> ver = new List<Gem>();
            ver.Add(center);
            if (lsXY.Equals("bellow"))
            {
                int yBelow = center.y - 1;
                while (yBelow >= 0)
                {
                    Gem gemBelow = gemAt(center.x, yBelow);
                    if (gemBelow != null)
                    {
                        if (!gemBelow.sameType(center))
                        {
                            break;
                        }
                        ver.Add(gemBelow);
                    }
                    yBelow--;
                }
            }
            if (lsXY.Equals("above"))
            {
                int yAbove = center.y + 1;
                while (yAbove < 8)
                {
                    Gem gemAbove = gemAt(center.x, yAbove);
                    if (gemAbove != null)
                    {
                        if (!gemAbove.sameType(center))
                        {
                            break;
                        }
                        ver.Add(gemAbove);
                    }
                    yAbove++;
                }
            }
            if (lsXY.Equals("left"))
            {
                int xLeft = center.x - 1;
                while (xLeft >= 0)
                {
                    Gem gemLeft = gemAt(xLeft, center.y);
                    if (gemLeft != null)
                    {
                        if (!gemLeft.sameType(center))
                        {
                            break;
                        }
                        ver.Add(gemLeft);
                    }
                    xLeft--;
                }
            }
            if (lsXY.Equals("right"))
            {
                int xRight = center.x + 1;
                while (xRight < 8)
                {
                    Gem gemRight = gemAt(xRight, center.y);
                    if (gemRight != null)
                    {
                        if (!gemRight.sameType(center))
                        {
                            break;
                        }
                        ver.Add(gemRight);
                    }
                    xRight++;
                }
            }
            return ver;
        }
        // Find Gem at Position (x, y)
        private Gem gemAt(int x, int y)
        {
            foreach (Gem g in gems)
            {
                if (g != null && g.x == x && g.y == y)
                {
                    return g;
                }
            }
            return null;
        }
    }
}