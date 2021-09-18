using System.Collections.Generic;
using System.Linq;
using Figures;
using Storage.FigureByTypes;
using UnityEngine;

namespace Storage
{
    [CreateAssetMenu(fileName = "FiguresStorage", menuName = "ScriptableObjects/FiguresStorage")]
    public class FiguresStorage : ScriptableObject
    {
        [SerializeField] private List<FigureByTypeParams> _figuresByTypeList;

        public Sprite GetSpriteByType(FigureType figureType)
        {
            return _figuresByTypeList.FirstOrDefault(figuresParams => figuresParams.FigureType == figureType)?.Sprite;
        }
    }
}