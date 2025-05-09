using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Model.Contexts;
using GameLogic.Model.DataProviders;
using Infrastructure;
using Infrastructure.Services;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace GameLogic.Model.Repositories
{
    public class UserContextRepository : IDisposable
    {
        [Inject] private IFileService _fileService;
        [Inject] private GameDefsDataProvider _gameDefs;

        public IReactiveProperty<string> LocalizationDefId => _localizationDefId;
        public IReactiveProperty<string> UpdatedLevelDefId => _localizationDefId;
        public IReactiveProperty<bool> IsSoundsMuted => _isSoundsMuted;

        private readonly ReactiveProperty<string> _localizationDefId = new();
        private readonly ReactiveProperty<string> _updatedLevelDefId = new();
        private readonly ReactiveProperty<bool> _isSoundsMuted = new();

        private readonly UserContext _userContext;
        private bool _willSave = false;


        public UserContextRepository(UserContext userContext)
        {
            _userContext = userContext;
            _localizationDefId.Value = userContext.LocalizationDefId;
            _isSoundsMuted.Value = userContext.IsSoundsMuted;
        }

        public void SetSoundsMuted(bool isMuted)
        {
            _userContext.IsSoundsMuted = isMuted;
            _isSoundsMuted.SetValueAndForceNotify(isMuted);
        }

        public void SetLocalization(string localizationDefId)
        {
            if (_gameDefs.Localizations.TryGetValue(localizationDefId, out _) == false)
            {
                Debug.LogWarning($"'{localizationDefId}' localization not found");
                localizationDefId = _userContext.LocalizationDefId ?? _gameDefs.DefaultSettings.LocalizationDefId;
            }
            _userContext.LocalizationDefId = localizationDefId;

            _localizationDefId.SetValueAndForceNotify(localizationDefId);
        }

        public void CompleteLevel(string levelDefId)
        {
            _userContext.LevelsProgress[levelDefId].IsCompleted = true;

            _updatedLevelDefId.SetValueAndForceNotify(levelDefId);
        }

        public void AddOrUpdateLevelProgress(string needLevelDefId, List<string> undistributedClusters, List<List<string>> distributedClusters)
        {
            if (_userContext.LevelsProgress.TryGetValue(needLevelDefId, out var needLevel) == false)
            {
                needLevel = new LevelProgressContext
                {
                    LevelDefId = needLevelDefId
                };
                _userContext.LevelsProgress.Add(needLevelDefId, needLevel);
            }
            needLevel.UndistributedClusters = undistributedClusters;
            needLevel.DistributedClusters = distributedClusters;

            _updatedLevelDefId.SetValueAndForceNotify(needLevelDefId);
        }

        public bool IsAnyLevelProgressExist()
        {
            return _userContext.LevelsProgress.Count > 0;
        }

        public bool IsLevelProgressExist(string needLevelDefId)
        {
            return _userContext.LevelsProgress.ContainsKey(needLevelDefId);
        }

        public bool TryGetLevelProgress(string needLevelDefId, out LevelProgressContext levelProgress)
        {
            return _userContext.LevelsProgress.TryGetValue(needLevelDefId, out levelProgress);
        }

        public bool IsLevelCompleted(string levelDefId)
        {
            return _userContext.LevelsProgress.TryGetValue(levelDefId, out var levelProgress) && levelProgress.IsCompleted;
        }

        public bool IsHowToPlayHintShown()
        {
            return _userContext.IsHowToPlayHintShown;
        }

        public void SetHowToPlayHintShown()
        {
            _userContext.IsHowToPlayHintShown = true;
        }

        public int GetAllFormedWordCount()
        {
            var result = 0;
            var localizationDefId = _userContext.LocalizationDefId;
            var levels = _gameDefs.Localizations[localizationDefId].Levels;
            foreach (var pair in levels)
            {
                if (_userContext.LevelsProgress.TryGetValue(pair.Value, out var levelProgress) == false)
                    continue;

                if (levelProgress.IsCompleted == false)
                    continue;

                result += _gameDefs.Levels[levelProgress.LevelDefId].Words.Count;
            }
            return result;
        }

        public void ClearProgress()
        {
            var localizationDefId = _userContext.LocalizationDefId;
            var levels = _gameDefs.Localizations[localizationDefId].Levels;
            foreach (var pair in levels)
            {
                _userContext.LevelsProgress.Remove(pair.Value);
            }
        }

        public async void Save()
        {
            if (_willSave) 
                return;

            _willSave = true;
            await UniTask.Yield();
            _willSave = false;

            SaveInternal();
        }

        private void SaveInternal()
        {
            var json = JsonConvert.SerializeObject(_userContext, Formatting.Indented);
            _fileService.WriteAllText(GamePaths.PlayerContext, json);
        }

        public void Dispose()
        {
            _localizationDefId.Dispose();
            _updatedLevelDefId.Dispose();
        }
    }
}