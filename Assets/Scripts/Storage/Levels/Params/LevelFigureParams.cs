using System;
using Figures;
using UnityEngine;

namespace Storage.Levels.Params
{
    [Serializable]
    public class LevelFigureParams
    {
        public FigureType FigureType;
        public float Scale;
        public Color Color;
        public Vector3 Position;
        public bool Completed;
    }
}