#if UNITY_EDITOR
using System.Linq;
using Configurations.Progress;
using NUnit.Framework;
using UnityEngine;

namespace Editor.Tests
{
    [TestFixture]
    public class ProgressConfigurationValidatorTests
    {
        private const string ValidCsvHeader = "PackId;PackName;StarsToUnlockPack;LevelId;LevelName;TimeA;TimeB;TimeC;FigureScale;Difficulty\n";

        private const string RealConfigResourceName = "ProgressConfiguration";

        private static ProgressConfiguration LoadRealConfiguration()
        {
            var textAsset = Resources.Load<TextAsset>(RealConfigResourceName);
            if (textAsset == null)
                return null;
            var config = new ProgressConfiguration();
            config.Parse(textAsset.text);
            return config;
        }

        [Test]
        public void Validate_RealConfiguration_FromResources_Passes()
        {
            var config = LoadRealConfiguration();
            Assert.IsNotNull(config, $"Resource '{RealConfigResourceName}' not found in Resources. Add ProgressConfiguration.csv to Assets/Resources.");
            Assert.IsNotNull(config.PacksInfoDictionary, "Parsed config has no packs.");

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count, "Real ProgressConfiguration should be valid. Errors: " + string.Join("; ", errors));
        }

        [Test]
        public void ValidateOrThrow_RealConfiguration_FromResources_DoesNotThrow()
        {
            var config = LoadRealConfiguration();
            Assert.IsNotNull(config, $"Resource '{RealConfigResourceName}' not found in Resources.");

            Assert.DoesNotThrow(() => ProgressConfigurationValidator.ValidateOrThrow(config));
        }

        [Test]
        public void Validate_ValidConfig_ReturnsNoErrors()
        {
            // TimeA < TimeB < TimeC: 3.11 < 3.88 < 5.82
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;3.11;3.88;5.82;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count, "Expected no errors. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_TimeA_NotLessThan_TimeB_ReturnsError()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;5.0;3.0;6.0;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("TimeA") || e.Contains("TimeB") || e.Contains("TimeC")), "Expected time order error. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_TimeB_NotLessThan_TimeC_ReturnsError()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;1;10;8;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("TimeA") || e.Contains("TimeB") || e.Contains("TimeC")), "Expected time order error.");
        }

        [Test]
        public void Validate_UniqueLevelIdPerLevel_NoDuplicateKeys()
        {
            // Parser dedupes (PackId, LevelId); validator ensures no duplicate (packId, levelId) in config
            var csv = ValidCsvHeader +
                      "1;pack1;0;1;Level1;3;4;5;2.5;1\n" +
                      "1;pack1;0;2;Level2;4;5;6;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.AreEqual(0, errors.Count, "Unique (PackId, LevelId) per row should pass. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_StarsToUnlockPack_Negative_ReturnsError()
        {
            var csv = ValidCsvHeader + "1;pack1;-1;1;Level1;3;4;5;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("StarsToUnlockPack") || e.Contains("StarsToUnlock")), "Expected StarsToUnlock error. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_FigureScale_ZeroOrNegative_ReturnsError()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;3;4;5;0;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("FigureScale")), "Expected FigureScale error. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_Difficulty_OutOfRange_ReturnsError()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;3;4;5;2.5;5\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            var errors = ProgressConfigurationValidator.Validate(config);

            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("Difficulty")), "Expected Difficulty error. Got: " + string.Join("; ", errors));
        }

        [Test]
        public void Validate_Difficulty_1_2_3_Accepted()
        {
            for (int d = 1; d <= 3; d++)
            {
                var csv = ValidCsvHeader + $"1;pack1;0;1;Level1;3;4;5;2.5;{d}\n";
                var config = new ProgressConfiguration();
                config.Parse(csv);
                var errors = ProgressConfigurationValidator.Validate(config);
                Assert.AreEqual(0, errors.Count, $"Difficulty {d} should be valid. Got: " + string.Join("; ", errors));
            }
        }

        [Test]
        public void Validate_NullConfig_ReturnsErrors()
        {
            var errors = ProgressConfigurationValidator.Validate(null);
            Assert.Greater(errors.Count, 0);
        }

        [Test]
        public void ValidateOrThrow_InvalidConfig_Throws()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;5;2;3;2.5;1\n"; // TimeA not < TimeB < TimeC
            var config = new ProgressConfiguration();
            config.Parse(csv);

            Assert.Throws<System.InvalidOperationException>(() => ProgressConfigurationValidator.ValidateOrThrow(config));
        }

        [Test]
        public void ValidateOrThrow_ValidConfig_DoesNotThrow()
        {
            var csv = ValidCsvHeader + "1;pack1;0;1;Level1;3;4;5;2.5;1\n";
            var config = new ProgressConfiguration();
            config.Parse(csv);

            Assert.DoesNotThrow(() => ProgressConfigurationValidator.ValidateOrThrow(config));
        }
    }
}
#endif
