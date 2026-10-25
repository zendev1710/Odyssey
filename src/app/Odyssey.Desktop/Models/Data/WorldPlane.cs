using Avalonia;
using Markdig.Syntax;
using Odyssey.Models.Data.Odyssey.Models.Data;
using Odyssey.Models.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Odyssey.Models.Data
{
    public enum PlaneType
    {
        WORLD = 0,
        ASTRAL = 1,
        ARENA = 1137,
        ETERNATH = 59034966,
        CHRISTMAS_ISLAND = 2000,
        OTHER = 5
    }

    public class WorldPlane
    {
        public string Name { get; }
        public PlaneType Type { get; }

        public Dictionary<int, RegionModel> OccupiedRegions { get; private set; } = [];
        public Dictionary<int, RegionModel> UnoccupiedRegions { get; private set; } = [];

        public WorldPlane(PlaneType planeType)
        {
            Type = planeType;
            Name = GetName(planeType);
        }

        /// <summary>
        /// Gets the bounds of the world plane based on the regions added to it.
        /// It defines an immutable rectangle that encompasses all the regions in the plane.
        /// </summary>
        public Bounds Bounds => (Bounds)_rectangle;

        // Mutable rectangle
        private Rectangle _rectangle = new();

        public void AddRegion(RegionModel regionModel, bool isOccupied)
        {
            Debug.WriteLine($"[DOCUMENT] Add region {regionModel} occupied={isOccupied}");
            regionModel.CollectData();
            RegionDataBlock? region = regionModel.Region;
            int planeId = region!.GetId();
            if (planeId == (int)Type)
            {
                int x = region.GetX();
                int y = region.GetY();
                int regionId = Coordinates.GetId(x, y, (PlaneType)planeId);
                if (_rectangle.IsEmpty)
                {
                    _rectangle.X = x;
                    _rectangle.Y = y;
                    _rectangle.Width = 1;
                    _rectangle.Height = 1;
                }
                else
                {   
                    _rectangle.X = Math.Min(_rectangle.X, x);
                    _rectangle.Width = Math.Max(_rectangle.Width, x - _rectangle.X + 1);
                    _rectangle.Y = Math.Min(_rectangle.Y, y);
                    _rectangle.Height = Math.Max(_rectangle.Height, y - _rectangle.Y + 1);
                }
                if (isOccupied)
                {
                    if (!OccupiedRegions.ContainsKey(regionId))
                    {
                        OccupiedRegions.Add(regionId, regionModel);
                    }
                    else
                    {
                        Debug.WriteLine($"[WORLDPLANE] populated region {regionModel} ({regionId}) already exists !!!");
                    }
                }
                else
                {
                    if (!UnoccupiedRegions.ContainsKey(regionId))
                    {
                        UnoccupiedRegions.Add(regionId, regionModel);
                    }
                    else
                    {
                        Debug.WriteLine($"[WORLDPLANE] empty region {regionModel} ({regionId}) already exists !!!");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"[WORLDPLANE] region {region} ({planeId}) must not be added to {Name} !!!");
            }
        }

        public static string GetName(PlaneType planeType)
        {
            switch (planeType)
            {
                case PlaneType.WORLD:
                    return Labels.Localize(Labels.PLANE_WORLD);
                case PlaneType.ASTRAL:
                    return Labels.Localize(Labels.PLANE_ASTRAL);
                case PlaneType.ARENA:
                    return Labels.Localize(Labels.PLANE_ARENA);
                case PlaneType.ETERNATH:
                    return Labels.Localize(Labels.PLANE_ETERNATH);
                case PlaneType.CHRISTMAS_ISLAND:
                    return Labels.Localize(Labels.PLANE_CHRISTMAS_ISLAND);
                default:
                    return Labels.Localize(Labels.PLANE_UNKNOWN, [$"{planeType}"]);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
