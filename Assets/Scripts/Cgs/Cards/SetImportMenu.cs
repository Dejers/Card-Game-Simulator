/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.IO;
using System.Linq;
using CardGameDef.Unity;
using Cgs.Menu;
using JetBrains.Annotations;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cgs.Cards
{
    public class SetImportMenu : Modal, IProgressible
    {
        public const string PlatformWarningMessage = "Sorry, Set Import is not supported from Windows Store!";

        public const string SelectFolderPrompt = "Select Folder";
        public const string ImportFolderWarningMessage = "No folder found for import! ";
        public const string ImportCardFailedWarningMessage = "Failed to find card: ";
        public const string ImportErrorMessage = "Error during import!: ";

        public Text setNameText;
        public TMP_Text cardNamesText;
        public Button importButton;

        public float ProgressPercentage { get; private set; }
        public string ProgressStatus { get; private set; }

        private string ImportStatus => $"Importing {SetName}...";

        private string SetName => Path.GetFileName(_setFolderPath);

        private string SetFolderPath
        {
            get => _setFolderPath;
            set
            {
                if (!Directory.Exists(value))
                {
                    Debug.LogWarning(ImportFolderWarningMessage + value);
                    CardGameManager.Instance.Messenger.Show(ImportFolderWarningMessage + value);
                    return;
                }

                _setFolderPath = value;

                Debug.Log("Import Set Folder Path set: " + _setFolderPath);
                setNameText.text = SetName;
                var cardNames = Directory.GetFiles(_setFolderPath)
                    .Where(filePath => filePath.EndsWith(CardGameManager.Current.CardImageFileType)).Aggregate(
                        string.Empty,
                        (current, filePath) =>
                        {
                            var fileName = Path.GetFileName(filePath);
                            var end = fileName.LastIndexOf(CardGameManager.Current.CardImageFileType,
                                StringComparison.Ordinal);
                            return current + fileName.Substring(0, end - 1) + "\n";
                        });
                cardNamesText.text = cardNames;
                ValidateImportButton();
            }
        }

        private string _setFolderPath;

        private UnityAction _onCreationCallback;

        public void Show(UnityAction onCreationCallback)
        {
            Show();
            _onCreationCallback = onCreationCallback;
        }

        private void Update()
        {
            if (!IsFocused)
                return;

            if ((Inputs.IsSubmit || Inputs.IsSave) && importButton.interactable)
                StartImport();
            if ((Inputs.IsNew || Inputs.IsLoad) && importButton.interactable)
                SelectFolder();
            else if (Inputs.IsCancel || Inputs.IsOption)
                Hide();
        }

        [UsedImplicitly]
        public void SelectFolder()
        {
#if ENABLE_WINMD_SUPPORT
            CardGameManager.Instance.Messenger.Show(PlatformWarningMessage);
            Hide();
            return;
#endif
            FileBrowser.ShowLoadDialog((paths) => { SetFolderPath = paths[0]; },
                () => { Debug.Log("FileBrowser Canceled"); }, FileBrowser.PickMode.Folders, false, null, null,
                SelectFolderPrompt);
        }

        private void ValidateImportButton()
        {
            importButton.interactable = Directory.Exists(SetFolderPath);
        }

        [UsedImplicitly]
        public void StartImport()
        {
            Debug.Log("Start Set Import: " + SetFolderPath);
            StartCoroutine(ImportSet());
        }

        private IEnumerator ImportSet()
        {
            ValidateImportButton();
            if (!importButton.interactable)
                yield break;

            importButton.interactable = false;
            yield return null;

            var setFilePath = Path.Combine(CardGameManager.Current.SetsDirectoryPath, SetName);
            if (!Directory.Exists(setFilePath))
                Directory.CreateDirectory(setFilePath);

            CardGameManager.Instance.Progress.Show(this);
            ProgressStatus = ImportStatus;

            var cardPathsToImport = Directory.GetFiles(_setFolderPath)
                .Where(filePath => filePath.EndsWith(CardGameManager.Current.CardImageFileType)).ToList();
            var cardCount = cardPathsToImport.Count;
            for (var i = 0; i < cardCount; i++)
            {
                try
                {
                    ProgressPercentage = (float) i / cardCount;
                    var fileName = Path.GetFileName(cardPathsToImport[i]);
                    var end = fileName.LastIndexOf(CardGameManager.Current.CardImageFileType,
                        StringComparison.Ordinal);
                    var cardName = fileName.Substring(0, end - 1);
                    var card = new UnityCard(CardGameManager.Current, cardName, cardName, SetName, null, false);
                    File.Copy(cardPathsToImport[i], card.ImageFilePath);

                    if (!File.Exists(card.ImageFilePath))
                    {
                        Debug.LogWarning(ImportCardFailedWarningMessage + card.Name);
                        CardGameManager.Instance.Messenger.Show(ImportCardFailedWarningMessage + card.Name);
                    }
                    else
                        CardGameManager.Current.Add(card, false);
                }
                catch
                {
                    Debug.LogWarning(ImportCardFailedWarningMessage + cardPathsToImport[i]);
                    CardGameManager.Instance.Messenger.Show(ImportCardFailedWarningMessage + cardPathsToImport[i]);
                }

                yield return null;
            }

            try
            {
                CardGameManager.Current.WriteAllCardsJson();
            }
            catch (Exception e)
            {
                Debug.LogError(ImportErrorMessage + e);
                CardGameManager.Instance.Messenger.Show(ImportErrorMessage + e);
            }

            CardGameManager.Instance.Progress.Hide();
            _onCreationCallback?.Invoke();

            ValidateImportButton();
            Hide();
        }
    }
}
