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

        public Dictionary<int, DataBlock> Regions { get; private set; } = [];

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

        /// <summary>
        /// Adds a region to the world plane if it does not already exist.
        /// At this moment, regions must be added in a plane only for special planes (astral...).
        /// </summary>
        /// <param name="region">region data block</param>
        /// <param name="regionId">region id</param>
        public void AddRegion(DataBlock region)
        {
            int planeId = region.GetId();
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
                if (!Regions.ContainsKey(regionId))
                {
                    Regions.Add(regionId, region);

                }
                else
                {
                    Debug.WriteLine($"[WORLDPLANE] region {region} ({regionId}) already exists !!!");
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
