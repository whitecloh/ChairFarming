using System;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    [Serializable]
    public sealed class RoutePoint
    {
        [SerializeField] private Vector2 position;
        [SerializeField] private RoutePointType pointType;
        [SerializeField] private int pinId = -1;
        [SerializeField] private int fingerIndex = -1;

        public Vector2 Position => position;
        public RoutePointType PointType => pointType;
        public int PinId => pinId;
        public int FingerIndex => fingerIndex;

        public RoutePoint(Vector2 position, RoutePointType pointType, int pinId = -1, int fingerIndex = -1)
        {
            this.position = position;
            this.pointType = pointType;
            this.pinId = pinId;
            this.fingerIndex = fingerIndex;
        }
    }
}
