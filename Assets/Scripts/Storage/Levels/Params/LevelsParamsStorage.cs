using System;
using System.Collections.Generic;
using System.Linq;
using Level.Hud;
using UnityEngine;

namespace Storage.Levels.Params
{
    [CreateAssetMenu(fileName = "LevelsParamsStorage", menuName = "ScriptableObjects/LevelsParams")]
    public class LevelsParamsStorage : ScriptableObject
    {
        [SerializeField] private List<PackParams> _defaultPackParamsList;

        public LevelParams GetDefaultLevelByNumber(int number)
        {
            return null;
        }

        public List<PackParams> DefaultPacksParamsList => _defaultPackParamsList;
    }
}