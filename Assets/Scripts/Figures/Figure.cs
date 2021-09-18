using Pooling;
using UnityEngine;

namespace Figures
{
    public abstract class Figure : MonoBehaviour, IFigure
    {
        [SerializeField] private FigureType _figureType;
        
        public FigureType FigureType { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsTarget { get; private set;}
        public int FigureScale { get; private set;}
        public PoolObject PoolObject { get; private set; }
        public Color FigureColor { get; private set;}

        public void SetFigureCompleted(bool value)
        {
            IsCompleted = value;
        }

        public void GetPoolObjectComponent()
        {
            var poolObj = GetComponent<PoolObject>();
            if (poolObj == null)
            {
                Debug.LogWarning($"Missing script {nameof(PoolObject)} reference in object with type {FigureType}");
            }

            PoolObject = poolObj;
        }

        public void SetUpDefaultParamsFigure(Color figureColor, FigureType type)
        {
            FigureColor = figureColor;
            FigureType = type;
        }
    }
}
