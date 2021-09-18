using System;
using Figures;
using Figures.Animals;
using UnityEngine;

namespace Storage.FigureByTypes
{
    [Serializable]
    public class FigureByTypeParams
    {
        public FigureType FigureType;
        public Sprite Sprite;
    }

    // [Serializable]
    // public class FiguresByType
    // {
    //     public FigureAnimalTarget FigureAnimal;
    //     public FigureAnimalsMenu FigureMenu;
    // }
}