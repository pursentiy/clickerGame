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
        [SerializeField] private List<LevelParams> defaultLevelParamsList;

        public LevelParams GetDefaultLevelByNumber(int number)
        {
            return defaultLevelParamsList.FirstOrDefault(levelParams => levelParams.LevelNumber == number);
        }

        public List<LevelParams> DefaultLevelsParamsList => defaultLevelParamsList;
    }
}