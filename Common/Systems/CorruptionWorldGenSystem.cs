using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CorruptionOverhaul.Common.Systems
{
    public class CorruptionWorldGenSystem : ModSystem
    {
        private struct CorruptionRegion
        {
            public int TileCount;
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public int LowestY;
            public int HighestY;
            public long CenterXSum;
            public long CenterYSum;

            public int CenterX
            {
                get
                {
                    if (TileCount <= 0)
                        return 0;

                    return (int)(CenterXSum / TileCount);
                }
            }

            public int CenterY
            {
                get
                {
                    if (TileCount <= 0)
                        return 0;

                    return (int)(CenterYSum / TileCount);
                }
            }

            public CorruptionRegion(int x, int y)
            {
                TileCount = 1;
                MinX = x;
                MaxX = x;
                MinY = y;
                MaxY = y;
                LowestY = y;
                HighestY = y;
                CenterXSum = x;
                CenterYSum = y;
            }

            public void Add(int x, int y)
            {
                TileCount++;

                if (x < MinX)
                    MinX = x;

                if (x > MaxX)
                    MaxX = x;

                if (y < MinY)
                    MinY = y;

                if (y > MaxY)
                    MaxY = y;

                if (y > LowestY)
                    LowestY = y;

                if (y < HighestY)
                    HighestY = y;

                CenterXSum += x;
                CenterYSum += y;
            }
        }

        private static readonly List<CorruptionRegion> Regions =
            new List<CorruptionRegion>();

        private static readonly List<Point> MainBoundary =
            new List<Point>();

        private static readonly List<Point> ExtensionSeeds =
            new List<Point>();

        private static byte[] visited;
        private static int[] floodQueue;
        private static int[] componentMap;

        private static int worldWidth;
        private static int worldHeight;

        private static int mainComponent;

        private static int mainRegionMinX;
        private static int mainRegionMaxX;
        private static int mainRegionMinY;
        private static int mainRegionMaxY;

        public override void PreWorldGen()
        {
            WorldGen.crimson = false;
        }

        public override void ModifyWorldGenTasks(
            List<GenPass> tasks,
            ref double totalWeight)
        {
            int corruptionIndex =
                tasks.FindIndex(
                    pass => pass.Name == "Corruption"
                );

            if (corruptionIndex == -1)
                return;

            tasks.Insert(
                corruptionIndex + 1,
                new PassLegacy(
                    "Corruption Overhaul",
                    ExtendVanillaCorruption
                )
            );

            int demoniteIndex =
                tasks.FindIndex(
                    pass => pass.Name == "Corruption Overhaul"
                );

            if (demoniteIndex == -1)
                return;

            tasks.Insert(
                demoniteIndex + 1,
                new PassLegacy(
                    "Corruption Demonite",
                    GenerateCorruptionDemonite
                )
            );
        }

        private void ExtendVanillaCorruption(
            GenerationProgress progress,
            GameConfiguration configuration)
        {
            worldWidth = Main.maxTilesX;
            worldHeight = Main.maxTilesY;

            PrepareMemory();

            progress.Message =
                "Studying the Corruption";

            FindCorruptionRegions();

            if (Regions.Count == 0)
            {
                ReleaseMemory();
                return;
            }

            progress.Message =
                "Mapping the Corruption";

            SelectMainRegion();

            progress.Message =
                "Mapping the Corruption edge";

            BuildMainBoundary();

            progress.Message =
                "Extending the Corruption";

            CreateExtensionSeeds();
            GenerateMainExtension();
            GenerateSideExtensions();
            GenerateDeepExtensions();

            progress.Message =
                "Finishing the Corruption";

            FinishExtension();

            ReleaseMemory();
        }

        private void PrepareMemory()
        {
            long tileCount =
                (long)worldWidth *
                worldHeight;

            if (tileCount > int.MaxValue)
            {
                throw new InvalidOperationException(
                    "World is too large for Corruption world generation."
                );
            }

            visited =
                new byte[(int)tileCount];

            floodQueue =
                new int[(int)tileCount];

            componentMap =
                new int[(int)tileCount];

            for (
                int i = 0;
                i < componentMap.Length;
                i++
            )
            {
                componentMap[i] = -1;
            }
        }

        private void ReleaseMemory()
        {
            visited = null;
            floodQueue = null;
            componentMap = null;

            Regions.Clear();
            MainBoundary.Clear();
            ExtensionSeeds.Clear();

            mainComponent = -1;
        }

        private int Index(int x, int y)
        {
            return
                y *
                worldWidth +
                x;
        }

        private bool IsEvilTile(Tile tile)
        {
            if (!tile.HasTile)
                return false;

            return
                tile.TileType == TileID.Ebonstone ||
                tile.TileType == TileID.CorruptGrass ||
                tile.TileType == TileID.Ebonsand ||
                tile.TileType == TileID.CorruptThorns;
        }

        private void FindCorruptionRegions()
        {
            for (
                int y = 10;
                y < worldHeight - 10;
                y++
            )
            {
                for (
                    int x = 10;
                    x < worldWidth - 10;
                    x++
                )
                {
                    int index =
                        Index(x, y);

                    if (visited[index] != 0)
                        continue;

                    Tile tile =
                        Main.tile[x, y];

                    if (!IsEvilTile(tile))
                    {
                        visited[index] = 1;
                        continue;
                    }

                    int regionId =
                        Regions.Count;

                    CorruptionRegion region =
                        FloodRegion(
                            x,
                            y,
                            regionId
                        );

                    Regions.Add(region);
                }
            }
        }

        private CorruptionRegion FloodRegion(
            int startX,
            int startY,
            int regionId)
        {
            int head = 0;
            int tail = 0;

            int startIndex =
                Index(
                    startX,
                    startY
                );

            floodQueue[tail++] =
                startIndex;

            visited[startIndex] = 1;

            componentMap[startIndex] =
                regionId;

            CorruptionRegion region =
                new CorruptionRegion(
                    startX,
                    startY
                );

            while (head < tail)
            {
                int current =
                    floodQueue[head++];

                int x =
                    current %
                    worldWidth;

                int y =
                    current /
                    worldWidth;

                if (!(x == startX &&
                      y == startY))
                {
                    region.Add(
                        x,
                        y
                    );
                }

                TryFloodNeighbour(
                    x - 1,
                    y,
                    regionId,
                    ref tail
                );

                TryFloodNeighbour(
                    x + 1,
                    y,
                    regionId,
                    ref tail
                );

                TryFloodNeighbour(
                    x,
                    y - 1,
                    regionId,
                    ref tail
                );

                TryFloodNeighbour(
                    x,
                    y + 1,
                    regionId,
                    ref tail
                );
            }

            return region;
        }

        private void TryFloodNeighbour(
            int x,
            int y,
            int regionId,
            ref int tail)
        {
            if (
                x < 1 ||
                x >= worldWidth - 1 ||
                y < 1 ||
                y >= worldHeight - 1
            )
            {
                return;
            }

            int index =
                Index(x, y);

            if (visited[index] != 0)
                return;

            visited[index] = 1;

            Tile tile =
                Main.tile[x, y];

            if (!IsEvilTile(tile))
                return;

            componentMap[index] =
                regionId;

            floodQueue[tail++] =
                index;
        }

        private void SelectMainRegion()
        {
            int largest = -1;
            int largestCount = -1;

            for (
                int i = 0;
                i < Regions.Count;
                i++
            )
            {
                CorruptionRegion region =
                    Regions[i];

                int score =
                    region.TileCount;

                int verticalBonus =
                    Math.Min(
                        10000,
                        Math.Max(
                            0,
                            region.LowestY -
                            region.HighestY
                        ) * 2
                    );

                score +=
                    verticalBonus;

                if (score > largestCount)
                {
                    largestCount =
                        score;

                    largest =
                        i;
                }
            }

            mainComponent =
                largest;

            CorruptionRegion main =
                Regions[
                    mainComponent
                ];

            mainRegionMinX =
                main.MinX;

            mainRegionMaxX =
                main.MaxX;

            mainRegionMinY =
                main.MinY;

            mainRegionMaxY =
                main.MaxY;
        }

        private void BuildMainBoundary()
        {
            MainBoundary.Clear();

            CorruptionRegion region =
                Regions[
                    mainComponent
                ];

            int minX =
                Math.Max(
                    1,
                    region.MinX - 2
                );

            int maxX =
                Math.Min(
                    worldWidth - 2,
                    region.MaxX + 2
                );

            int minY =
                Math.Max(
                    1,
                    region.MinY - 2
                );

            int maxY =
                Math.Min(
                    worldHeight - 2,
                    region.MaxY + 2
                );

            for (
                int y = minY;
                y <= maxY;
                y++
            )
            {
                for (
                    int x = minX;
                    x <= maxX;
                    x++
                )
                {
                    int index =
                        Index(x, y);

                    if (
                        componentMap[index] !=
                        mainComponent
                    )
                    {
                        continue;
                    }

                    if (!IsBoundaryTile(x, y))
                        continue;

                    MainBoundary.Add(
                        new Point(
                            x,
                            y
                        )
                    );
                }
            }
        }

        private bool IsBoundaryTile(
            int x,
            int y)
        {
            if (!IsMainCorruption(x, y))
                return false;

            if (!IsMainCorruption(x - 1, y))
                return true;

            if (!IsMainCorruption(x + 1, y))
                return true;

            if (!IsMainCorruption(x, y - 1))
                return true;

            if (!IsMainCorruption(x, y + 1))
                return true;

            return false;
        }

        private bool IsMainCorruption(
            int x,
            int y)
        {
            if (
                x < 1 ||
                x >= worldWidth - 1 ||
                y < 1 ||
                y >= worldHeight - 1
            )
            {
                return false;
            }

            return
                componentMap[
                    Index(x, y)
                ] ==
                mainComponent;
        }

        private void CreateExtensionSeeds()
        {
            ExtensionSeeds.Clear();

            if (MainBoundary.Count == 0)
                return;

            CorruptionRegion region =
                Regions[
                    mainComponent
                ];

            int lowestY =
                region.LowestY;

            int deepestStart =
                Math.Max(
                    region.HighestY,
                    lowestY - 80
                );

            for (
                int i = 0;
                i < 350;
                i++
            )
            {
                Point point =
                    MainBoundary[
                        WorldGen.genRand.Next(
                            MainBoundary.Count
                        )
                    ];

                if (
                    point.Y <
                    deepestStart
                )
                {
                    continue;
                }

                ExtensionSeeds.Add(point);
            }

            if (ExtensionSeeds.Count == 0)
            {
                for (
                    int i = 0;
                    i < MainBoundary.Count &&
                    ExtensionSeeds.Count < 20;
                    i++
                )
                {
                    ExtensionSeeds.Add(
                        MainBoundary[i]
                    );
                }
            }
        }

        private void GenerateMainExtension()
        {
            if (ExtensionSeeds.Count == 0)
                return;

            Point seed =
                FindDeepestSeed();

            float x =
                seed.X;

            float y =
                seed.Y;

            float direction =
                MathHelper.PiOver2 +
                WorldGen.genRand.NextFloat(
                    -0.12f,
                    0.12f
                );

            int targetDepth =
                Math.Min(
                    worldHeight - 250,
                    Math.Max(
                        mainRegionMaxY + 250,
                        (int)GenVars.rockLayer + 850
                    )
                );

            int steps =
                Math.Max(
                    80,
                    (targetDepth - seed.Y) / 2
                );

            for (
                int i = 0;
                i < steps;
                i++
            )
            {
                if (y >= targetDepth)
                    break;

                float depth =
                    i /
                    (float)steps;

                int center =
                    GetVanillaContinuationCenter(
                        (int)y
                    );

                float pull =
                    center - x;

                direction +=
                    pull *
                    0.0017f;

                direction +=
                    WorldGen.genRand.NextFloat(
                        -0.035f,
                        0.035f
                    );

                x +=
                    (float)Math.Cos(direction) *
                    2.0f;

                y +=
                    (float)Math.Sin(direction) *
                    2.95f;

                if (!WorldGen.InWorld(
                    (int)x,
                    (int)y,
                    24))
                {
                    break;
                }

                int radius =
                    (int)MathHelper.Lerp(
                        5f,
                        13f,
                        depth
                    );

                if (depth > 0.72f)
                {
                    radius +=
                        WorldGen.genRand.Next(
                            1,
                            3
                        );
                }

                if (i % 37 == 0)
                {
                    radius +=
                        WorldGen.genRand.Next(
                            2,
                            5
                        );
                }

                ConvertArea(
                    (int)x,
                    (int)y,
                    radius
                );

                AddWallRelief(
                    (int)x,
                    (int)y,
                    direction,
                    radius
                );

                if (depth > 0.78f &&
                    i % 11 == 0)
                {
                    AddBottomExpansion(
                        (int)x,
                        (int)y,
                        radius,
                        depth
                    );
                }
            }
        }

        private Point FindDeepestSeed()
        {
            Point deepest =
                ExtensionSeeds[0];

            for (
                int i = 1;
                i < ExtensionSeeds.Count;
                i++
            )
            {
                if (
                    ExtensionSeeds[i].Y >
                    deepest.Y
                )
                {
                    deepest =
                        ExtensionSeeds[i];
                }
            }

            return deepest;
        }

        private int GetVanillaContinuationCenter(
            int y)
        {
            int center =
                Regions[
                    mainComponent
                ].CenterX;

            float depth =
                y /
                (float)worldHeight;

            float curve =
                (float)Math.Sin(
                    y * 0.010f
                ) * 52f;

            curve +=
                (float)Math.Sin(
                    y * 0.026f + 2f
                ) * 21f;

            curve +=
                (float)Math.Sin(
                    y * 0.0045f + 1.2f
                ) * 32f;

            curve *=
                0.65f +
                depth * 0.8f;

            return
                center +
                (int)curve;
        }

        private void GenerateSideExtensions()
        {
            int count =
                Main.maxTilesX >= 7000
                    ? 8
                    : 6;

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                if (MainBoundary.Count == 0)
                    break;

                Point seed =
                    MainBoundary[
                        WorldGen.genRand.Next(
                            MainBoundary.Count
                        )
                    ];

                if (
                    seed.Y <
                    corruptionSurfaceY()
                )
                {
                    continue;
                }

                GenerateSideBranch(
                    seed,
                    WorldGen.genRand.NextBool()
                );
            }
        }

        private int corruptionSurfaceY()
        {
            return
                (int)Main.worldSurface +
                80;
        }

        private void GenerateSideBranch(
            Point seed,
            bool left)
        {
            float x =
                seed.X;

            float y =
                seed.Y;

            float direction =
                left
                    ? WorldGen.genRand.NextFloat(
                        MathHelper.Pi + 0.25f,
                        MathHelper.TwoPi - 0.25f
                    )
                    : WorldGen.genRand.NextFloat(
                        0.25f,
                        MathHelper.Pi - 0.25f
                    );

            int steps =
                WorldGen.genRand.Next(
                    80,
                    150
                );

            for (
                int i = 0;
                i < steps;
                i++
            )
            {
                direction +=
                    WorldGen.genRand.NextFloat(
                        -0.065f,
                        0.065f
                    );

                x +=
                    (float)Math.Cos(direction) *
                    2.1f;

                y +=
                    (float)Math.Sin(direction) *
                    1.8f;

                if (!WorldGen.InWorld(
                    (int)x,
                    (int)y,
                    20))
                {
                    break;
                }

                int radius =
                    WorldGen.genRand.Next(
                        4,
                        9
                    );

                ConvertArea(
                    (int)x,
                    (int)y,
                    radius
                );

                AddWallRelief(
                    (int)x,
                    (int)y,
                    direction,
                    radius
                );

                if (i % 24 == 0)
                {
                    ConvertArea(
                        (int)x,
                        (int)y,
                        radius + 4
                    );
                }

                if (i > steps * 0.7f &&
                    i % 14 == 0)
                {
                    AddBottomExpansion(
                        (int)x,
                        (int)y,
                        radius,
                        i / (float)steps
                    );
                }
            }
        }

        private void GenerateDeepExtensions()
        {
            int count =
                Main.maxTilesX >= 7000
                    ? 5
                    : 3;

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                if (ExtensionSeeds.Count == 0)
                    break;

                Point seed =
                    ExtensionSeeds[
                        WorldGen.genRand.Next(
                            ExtensionSeeds.Count
                        )
                    ];

                GenerateDeepBranch(
                    seed
                );
            }
        }

        private void GenerateDeepBranch(
            Point seed)
        {
            float x =
                seed.X;

            float y =
                seed.Y;

            float direction =
                MathHelper.PiOver2 +
                WorldGen.genRand.NextFloat(
                    -0.35f,
                    0.35f
                );

            int steps =
                WorldGen.genRand.Next(
                    100,
                    180
                );

            for (
                int i = 0;
                i < steps;
                i++
            )
            {
                direction +=
                    WorldGen.genRand.NextFloat(
                        -0.065f,
                        0.065f
                    );

                int center =
                    GetVanillaContinuationCenter(
                        (int)y
                    );

                direction +=
                    (center - x) *
                    0.0013f;

                x +=
                    (float)Math.Cos(direction) *
                    2.0f;

                y +=
                    (float)Math.Sin(direction) *
                    2.7f;

                if (!WorldGen.InWorld(
                    (int)x,
                    (int)y,
                    18))
                {
                    break;
                }

                int radius =
                    WorldGen.genRand.Next(
                        4,
                        8
                    );

                ConvertArea(
                    (int)x,
                    (int)y,
                    radius
                );

                AddWallRelief(
                    (int)x,
                    (int)y,
                    direction,
                    radius
                );

                if (i % 28 == 0)
                {
                    ConvertArea(
                        (int)x,
                        (int)y,
                        radius + 4
                    );
                }

                if (i > steps * 0.7f &&
                    i % 13 == 0)
                {
                    AddBottomExpansion(
                        (int)x,
                        (int)y,
                        radius,
                        i / (float)steps
                    );
                }
            }
        }

        private void AddWallRelief(
            int x,
            int y,
            float direction,
            int tunnelRadius)
        {
            if (WorldGen.genRand.Next(100) >= 32)
                return;

            float side =
                WorldGen.genRand.NextBool()
                    ? -1f
                    : 1f;

            float sideX =
                -(float)Math.Sin(direction) *
                side;

            float sideY =
                (float)Math.Cos(direction) *
                side;

            int distance =
                tunnelRadius +
                WorldGen.genRand.Next(
                    2,
                    5
                );

            int reliefX =
                x +
                (int)(sideX * distance);

            int reliefY =
                y +
                (int)(sideY * distance);

            int reliefRadius =
                WorldGen.genRand.Next(
                    2,
                    5
                );

            if (!WorldGen.InWorld(
                reliefX,
                reliefY,
                reliefRadius + 5))
            {
                return;
            }

            if (WorldGen.genRand.NextBool(2))
            {
                ConvertArea(
                    reliefX,
                    reliefY,
                    reliefRadius
                );
            }
            else
            {
                ConvertArea(
                    reliefX,
                    reliefY,
                    reliefRadius + 1
                );

                int secondX =
                    reliefX -
                    (int)(sideX * 2f);

                int secondY =
                    reliefY -
                    (int)(sideY * 2f);

                ConvertArea(
                    secondX,
                    secondY,
                    Math.Max(
                        2,
                        reliefRadius - 1
                    )
                );
            }
        }

        private void AddBottomExpansion(
            int x,
            int y,
            int radius,
            float depth)
        {
            int extra =
                WorldGen.genRand.Next(
                    3,
                    7
                );

            int expansionRadius =
                Math.Min(
                    17,
                    radius +
                    extra +
                    (int)(depth * 3f)
                );

            ConvertArea(
                x,
                y + radius / 2,
                expansionRadius
            );

            if (WorldGen.genRand.NextBool(2))
            {
                ConvertArea(
                    x - WorldGen.genRand.Next(2, 6),
                    y + radius,
                    Math.Max(
                        4,
                        expansionRadius - 3
                    )
                );
            }

            if (WorldGen.genRand.NextBool(2))
            {
                ConvertArea(
                    x + WorldGen.genRand.Next(2, 6),
                    y + radius,
                    Math.Max(
                        4,
                        expansionRadius - 3
                    )
                );
            }
        }

        private void ConvertArea(
            int x,
            int y,
            int radius)
        {
            if (!WorldGen.InWorld(
                x,
                y,
                radius + 3))
            {
                return;
            }

            if (TouchesProtectedBiome(
                x,
                y,
                radius))
            {
                return;
            }

            if (TouchesVanillaStructure(
                x,
                y,
                radius))
            {
                return;
            }

            WorldGen.Convert(
                x,
                y,
                1,
                radius,
                true,
                true
            );
        }

        private bool TouchesVanillaStructure(
            int centerX,
            int centerY,
            int radius)
        {
            int startX =
                Math.Max(
                    5,
                    centerX - radius - 2
                );

            int endX =
                Math.Min(
                    worldWidth - 6,
                    centerX + radius + 2
                );

            int startY =
                Math.Max(
                    5,
                    centerY - radius - 2
                );

            int endY =
                Math.Min(
                    worldHeight - 6,
                    centerY + radius + 2
                );

            for (
                int y = startY;
                y <= endY;
                y++
            )
            {
                for (
                    int x = startX;
                    x <= endX;
                    x++
                )
                {
                    Tile tile =
                        Main.tile[x, y];

                    if (!tile.HasTile)
                        continue;

                    ushort type =
                        tile.TileType;

                    if (
                        type == TileID.DemonAltar ||
                        type == TileID.ShadowOrbs
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TouchesProtectedBiome(
            int centerX,
            int centerY,
            int radius)
        {
            int startX =
                Math.Max(
                    5,
                    centerX - radius - 3
                );

            int endX =
                Math.Min(
                    worldWidth - 6,
                    centerX + radius + 3
                );

            int startY =
                Math.Max(
                    5,
                    centerY - radius - 3
                );

            int endY =
                Math.Min(
                    worldHeight - 6,
                    centerY + radius + 3
                );

            for (
                int y = startY;
                y <= endY;
                y++
            )
            {
                for (
                    int x = startX;
                    x <= endX;
                    x++
                )
                {
                    Tile tile =
                        Main.tile[x, y];

                    if (!tile.HasTile)
                        continue;

                    ushort type =
                        tile.TileType;

                    if (
                        type == TileID.JungleGrass ||
                        type == TileID.Mud ||
                        type == TileID.JunglePlants ||
                        type == TileID.JunglePlants2 ||
                        type == TileID.JungleThorns ||
                        type == TileID.JungleVines
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.SnowBlock ||
                        type == TileID.SnowBrick ||
                        type == TileID.IceBlock ||
                        type == TileID.IceBrick
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Sand ||
                        type == TileID.HardenedSand ||
                        type == TileID.Sandstone ||
                        type == TileID.DesertFossil
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Granite ||
                        type == TileID.GraniteBlock ||
                        type == TileID.Marble ||
                        type == TileID.MarbleBlock
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.MushroomGrass ||
                        type == TileID.MushroomBlock
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Hive ||
                        type == TileID.HoneyBlock ||
                        type == TileID.LihzahrdBrick ||
                        type == TileID.LihzahrdAltar
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Meteorite ||
                        type == TileID.Obsidian ||
                        type == TileID.Hellstone ||
                        type == TileID.HellstoneBrick
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.BlueDungeonBrick ||
                        type == TileID.GreenDungeonBrick ||
                        type == TileID.PinkDungeonBrick ||
                        type == TileID.CrackedBlueDungeonBrick ||
                        type == TileID.CrackedGreenDungeonBrick ||
                        type == TileID.CrackedPinkDungeonBrick
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Crimstone ||
                        type == TileID.CrimsonGrass ||
                        type == TileID.Crimsand ||
                        type == TileID.CrimsonHardenedSand ||
                        type == TileID.CrimsonSandstone
                    )
                    {
                        return true;
                    }

                    if (
                        type == TileID.Pearlstone ||
                        type == TileID.PearlstoneBrick ||
                        type == TileID.HallowedGrass ||
                        type == TileID.HallowedIce ||
                        type == TileID.HallowHardenedSand ||
                        type == TileID.HallowSandstone
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void GenerateCorruptionDemonite(
            GenerationProgress progress,
            GameConfiguration configuration)
        {
            progress.Message =
                "Growing Demonite beneath the Corruption";

            List<Point> ebonTargets =
                new List<Point>();

            CollectDemoniteTargets(
                ebonTargets
            );

            if (ebonTargets.Count == 0)
                return;

            int worldSize =
                Main.maxTilesX *
                Main.maxTilesY;

            int depositCount =
                (int)(
                    worldSize *
                    0.00016
                );

            if (Main.maxTilesX >= 7000)
            {
                depositCount =
                    (int)(
                        worldSize *
                        0.00019
                    );
            }

            depositCount =
                Math.Max(
                    depositCount,
                    180
                );

            int successfulDeposits = 0;

            int attempts =
                depositCount * 5;

            for (
                int i = 0;
                i < attempts &&
                successfulDeposits < depositCount;
                i++
            )
            {
                Point target =
                    ebonTargets[
                        WorldGen.genRand.Next(
                            ebonTargets.Count
                        )
                    ];

                if (!TryPlaceDemoniteDeposit(
                    target
                ))
                {
                    continue;
                }

                successfulDeposits++;
            }

            FrameDemoniteArea();
        }

        private void CollectDemoniteTargets(
            List<Point> targets)
        {
            int step = 2;

            for (
                int y = 20;
                y < Main.maxTilesY - 220;
                y += step
            )
            {
                for (
                    int x = 10;
                    x < Main.maxTilesX - 10;
                    x += step
                )
                {
                    Tile tile =
                        Main.tile[x, y];

                    if (!tile.HasTile)
                        continue;

                    if (
                        tile.TileType != TileID.Ebonstone &&
                        tile.TileType != TileID.Ebonsand
                    )
                    {
                        continue;
                    }

                    if (!HasSolidGroundBelow(
                        x,
                        y
                    ))
                    {
                        continue;
                    }

                    targets.Add(
                        new Point(
                            x,
                            y
                        )
                    );
                }
            }
        }

        private bool HasSolidGroundBelow(
            int x,
            int y)
        {
            if (!WorldGen.InWorld(
                x,
                y + 1,
                1))
            {
                return false;
            }

            Tile below =
                Main.tile[x, y + 1];

            if (!below.HasTile)
                return false;

            if (
                below.TileType == TileID.Ebonstone ||
                below.TileType == TileID.Ebonsand
            )
            {
                return false;
            }

            return IsDemoniteHostTile(
                below.TileType
            );
        }

        private bool TryPlaceDemoniteDeposit(
            Point ebonTarget)
        {
            int direction =
                WorldGen.genRand.NextBool()
                    ? -1
                    : 1;

            int offset =
                WorldGen.genRand.Next(
                    2,
                    9
                );

            int startX =
                ebonTarget.X +
                WorldGen.genRand.Next(
                    -6,
                    7
                );

            int startY =
                ebonTarget.Y +
                WorldGen.genRand.Next(
                    1,
                    5
                );

            if (!WorldGen.InWorld(
                startX,
                startY,
                12))
            {
                return false;
            }

            Point host =
                FindDemoniteHostBelow(
                    startX,
                    startY,
                    direction,
                    offset
                );

            if (host.X == -1)
                return false;

            int strength =
                WorldGen.genRand.Next(
                    3,
                    7
                );

            int steps =
                WorldGen.genRand.Next(
                    4,
                    9
                );

            int before =
                CountDemoniteNearby(
                    host.X,
                    host.Y,
                    14
                );

            WorldGen.OreRunner(
                host.X,
                host.Y,
                strength,
                steps,
                TileID.Demonite
            );

            int after =
                CountDemoniteNearby(
                    host.X,
                    host.Y,
                    14
                );

            if (after <= before)
                return false;

            return true;
        }

        private Point FindDemoniteHostBelow(
            int startX,
            int startY,
            int direction,
            int offset)
        {
            int x =
                startX +
                direction *
                offset;

            int maxDepth =
                Math.Min(
                    Main.maxTilesY - 230,
                    startY + 160
                );

            for (
                int y = startY;
                y < maxDepth;
                y++
            )
            {
                if (!WorldGen.InWorld(
                    x,
                    y,
                    5))
                {
                    return new Point(
                        -1,
                        -1
                    );
                }

                Tile tile =
                    Main.tile[x, y];

                if (!tile.HasTile)
                    continue;

                if (
                    tile.TileType == TileID.Ebonstone ||
                    tile.TileType == TileID.Ebonsand
                )
                {
                    continue;
                }

                if (!IsDemoniteHostTile(
                    tile.TileType
                ))
                {
                    continue;
                }

                return new Point(
                    x,
                    y
                );
            }

            return new Point(
                -1,
                -1
            );
        }

        private bool IsDemoniteHostTile(
            ushort type)
        {
            return
                type == TileID.Stone ||
                type == TileID.Dirt ||
                type == TileID.Mud ||
                type == TileID.Silt ||
                type == TileID.Slush ||
                type == TileID.Sandstone ||
                type == TileID.HardenedSand;
        }

        private int CountDemoniteNearby(
            int centerX,
            int centerY,
            int radius)
        {
            int count = 0;

            int startX =
                Math.Max(
                    1,
                    centerX - radius
                );

            int endX =
                Math.Min(
                    Main.maxTilesX - 2,
                    centerX + radius
                );

            int startY =
                Math.Max(
                    1,
                    centerY - radius
                );

            int endY =
                Math.Min(
                    Main.maxTilesY - 2,
                    centerY + radius
                );

            for (
                int y = startY;
                y <= endY;
                y++
            )
            {
                for (
                    int x = startX;
                    x <= endX;
                    x++
                )
                {
                    Tile tile =
                        Main.tile[x, y];

                    if (
                        tile.HasTile &&
                        tile.TileType == TileID.Demonite
                    )
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void FrameDemoniteArea()
        {
            int minX =
                Math.Max(
                    10,
                    mainRegionMinX - 250
                );

            int maxX =
                Math.Min(
                    Main.maxTilesX - 10,
                    mainRegionMaxX + 250
                );

            int minY =
                Math.Max(
                    10,
                    mainRegionMinY
                );

            int maxY =
                Math.Min(
                    Main.maxTilesY - 220,
                    mainRegionMaxY + 350
                );

            WorldGen.SectionTileFrame(
                minX,
                minY,
                maxX,
                maxY
            );
        }

        private void FinishExtension()
        {
            int minX =
                Math.Max(
                    10,
                    Math.Min(
                        mainRegionMinX - 80,
                        corruptionCenterX() - 500
                    )
                );

            int maxX =
                Math.Min(
                    worldWidth - 10,
                    Math.Max(
                        mainRegionMaxX + 80,
                        corruptionCenterX() + 500
                    )
                );

            int minY =
                Math.Max(
                    10,
                    mainRegionMinY - 40
                );

            int maxY =
                Math.Min(
                    worldHeight - 10,
                    Math.Max(
                        mainRegionMaxY + 100,
                        (int)GenVars.rockLayer + 900
                    )
                );

            WorldGen.SectionTileFrame(
                minX,
                minY,
                maxX,
                maxY
            );
        }

        private int corruptionCenterX()
        {
            return
                Regions[
                    mainComponent
                ].CenterX;
        }
    }
}